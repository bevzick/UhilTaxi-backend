using Microsoft.EntityFrameworkCore;
using UhilTaxi.Application.Exceptions;
using UhilTaxi.Domain.Entities;
using UhilTaxi.Domain.Enums;
using UhilTaxi.Infrastructure.Persistence;

namespace UhilTaxi.Infrastructure.Operations;

// Application-facing use cases; controllers never directly access the database.
public sealed class OperationsService(UhilTaxiDbContext db)
{
    private static AuthException Bad(string code, string msg) => new(400, code, msg);
    private static AuthException Conflict(string code, string msg) => new(409, code, msg);
    private static AuthException Missing() => new(404, "NOT_FOUND", "Resource not found.");
    private async Task<T> Find<T>(long id, CancellationToken ct) where T: class
        => await db.Set<T>().FindAsync(new object[] { id }, ct) ?? throw Missing();
    private void RecordAudit(long actorId, string action, string entityType, long? entityId, object? metadata = null)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = actorId, Action = action, EntityType = entityType, EntityId = entityId,
            Metadata = metadata is null ? null : System.Text.Json.JsonSerializer.Serialize(metadata),
            CreatedAt = DateTime.UtcNow
        });
    }
    private static readonly string[] Categories = ["economy", "standard", "business", "xl"];
    private static readonly string[] Fuels = ["petrol", "diesel", "electric", "hybrid"];
    public Task<List<CarModel>> Models(CancellationToken ct) => db.CarModels.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    public async Task<CarModel> AddModel(CarModel m, CancellationToken ct, long actorId)
    {
        if (string.IsNullOrWhiteSpace(m.Brand) || string.IsNullOrWhiteSpace(m.ModelName) || !Categories.Contains(m.Category) || !Fuels.Contains(m.FuelType) || m.SeatCount < 1) throw Bad("INVALID_CAR_MODEL", "Invalid car model parameters.");
        if (await db.CarModels.AnyAsync(x => x.Brand == m.Brand && x.ModelName == m.ModelName && x.FuelType == m.FuelType, ct)) throw Conflict("CAR_MODEL_EXISTS", "Car model already exists.");
        m.Id=0;db.CarModels.Add(m); await db.SaveChangesAsync(ct); RecordAudit(actorId,"CAR_MODEL_CREATED","car_model",m.Id);await db.SaveChangesAsync(ct); return m;
    }
    public async Task<CarModel> UpdateModel(long id, CarModel request, CancellationToken ct, long actorId)
    {
        var m = await Find<CarModel>(id,ct);
        if (string.IsNullOrWhiteSpace(request.Brand) || string.IsNullOrWhiteSpace(request.ModelName) || !Categories.Contains(request.Category) || !Fuels.Contains(request.FuelType) || request.SeatCount < 1) throw Bad("INVALID_CAR_MODEL", "Invalid car model parameters.");
        m.Brand=request.Brand; m.ModelName=request.ModelName; m.Category=request.Category; m.FuelType=request.FuelType; m.SeatCount=request.SeatCount;RecordAudit(actorId,"CAR_MODEL_UPDATED","car_model",id);
        await db.SaveChangesAsync(ct);return m;
    }
    public Task<List<Car>> Cars(CancellationToken ct) => db.Cars.AsNoTracking().OrderBy(x=>x.Id).ToListAsync(ct);
    public Task<Car> Car(long id,CancellationToken ct)=>Find<Car>(id,ct);
    public async Task<Car> AddCar(Car c,CancellationToken ct,long actorId)
    {
        if (!await db.CarModels.AnyAsync(x=>x.Id==c.ModelId,ct)) throw Missing();
        CheckCar(c);
        if(await db.Cars.AnyAsync(x=>x.VinCode==c.VinCode || x.LicensePlate==c.LicensePlate,ct))throw Conflict("CAR_EXISTS","Plate or VIN already exists.");
        c.Status="active";c.CreatedAt=DateTime.UtcNow;c.UpdatedAt=c.CreatedAt;db.Cars.Add(c);await db.SaveChangesAsync(ct);RecordAudit(actorId,"CAR_CREATED","car",c.Id);await db.SaveChangesAsync(ct);return c;
    }
    private static void CheckCar(Car c)
    { if(string.IsNullOrWhiteSpace(c.LicensePlate)||string.IsNullOrWhiteSpace(c.VinCode)||c.VinCode.Length!=17 || c.Year<1980 || c.Year>DateTime.UtcNow.Year+1 || string.IsNullOrWhiteSpace(c.Color))throw Bad("INVALID_CAR","Invalid car details."); }
    public async Task<Car> UpdateCar(long id,Car input,CancellationToken ct,long actorId)
    {
        var c=await Find<Car>(id,ct);CheckCar(input);
        if(!await db.CarModels.AnyAsync(x=>x.Id==input.ModelId,ct))throw Missing();
        if(await db.Cars.AnyAsync(x=>x.Id!=id && (x.VinCode==input.VinCode||x.LicensePlate==input.LicensePlate),ct))throw Conflict("CAR_EXISTS","Plate or VIN already exists.");
        c.ModelId=input.ModelId;c.LicensePlate=input.LicensePlate;c.VinCode=input.VinCode;c.Year=input.Year;c.Color=input.Color;c.UpdatedAt=DateTime.UtcNow;RecordAudit(actorId,"CAR_UPDATED","car",id);
        await db.SaveChangesAsync(ct);return c;
    }
    public async Task<Car> SetCarStatus(long id,string status,CancellationToken ct,long actorId)
    {
        if(status is not ("active" or "maintenance" or "out_of_service"))throw Bad("INVALID_CAR_STATUS","Invalid status.");
        var c=await Find<Car>(id,ct);
        if(status!="active" && await db.Shifts.AnyAsync(x=>x.CarId==id && x.Status=="open",ct))throw Conflict("CAR_HAS_OPEN_SHIFT","Close the shift first.");
        c.Status=status;c.UpdatedAt=DateTime.UtcNow;RecordAudit(actorId,"CAR_STATUS_CHANGED","car",id,new { status });await db.SaveChangesAsync(ct);return c;
    }
    public Task<List<Shift>> Shifts(CancellationToken ct)=>db.Shifts.AsNoTracking().OrderByDescending(x=>x.StartTime).ToListAsync(ct);
    public Task<Shift> Shift(long id,CancellationToken ct)=>Find<Shift>(id,ct);
    public Task<List<Shift>> DriverShifts(long driverId,CancellationToken ct)=>db.Shifts.AsNoTracking().Where(x=>x.DriverId==driverId).OrderByDescending(x=>x.StartTime).ToListAsync(ct);
    public Task<Shift?> CurrentShift(long driverId,CancellationToken ct)=>db.Shifts.AsNoTracking().FirstOrDefaultAsync(x=>x.DriverId==driverId && x.Status=="open",ct);
    public async Task<Shift> OpenShift(long driverId,long carId,int mileage,CancellationToken ct)
    {
        if(mileage<0)throw Bad("INVALID_MILEAGE","Mileage must be nonnegative.");
        await using var transaction=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
        var driver=await db.Users.AnyAsync(x=>x.Id==driverId && x.Role==UserRole.Driver && x.Status==UserStatus.Active,ct);
        if(!driver)throw Conflict("DRIVER_NOT_ACTIVE","Active driver required.");
        var car=await db.Cars.FirstOrDefaultAsync(x=>x.Id==carId,ct);
        if(car is null||car.Status!="active")throw Conflict("CAR_NOT_ACTIVE","Active car required.");
        if(await db.Shifts.AnyAsync(x=>x.Status=="open" && (x.DriverId==driverId||x.CarId==carId),ct)) throw Conflict("SHIFT_CONFLICT","Driver or car already has an open shift.");
        var shift=new Shift{DriverId=driverId,CarId=carId,StartTime=DateTime.UtcNow,StartMileage=mileage,Status="open"};
        db.Shifts.Add(shift);await db.SaveChangesAsync(ct);await transaction.CommitAsync(ct);return shift;
    }
    public async Task<Shift> CloseShift(long id,long driverId,int mileage,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var shift=await db.Shifts.FirstOrDefaultAsync(x=>x.Id==id && x.DriverId==driverId,ct)??throw Missing();
        if(shift.Status!="open")throw Conflict("SHIFT_CLOSED","Shift is not open.");
        if(mileage<shift.StartMileage)throw Bad("INVALID_MILEAGE","End mileage must be at least start mileage.");
        if(await db.Trips.AnyAsync(x=>x.ShiftId==id && x.ActualEndTime==null,ct))throw Conflict("TRIP_IN_PROGRESS","Finish the trip before closing shift.");
        shift.Status="closed";shift.EndTime=DateTime.UtcNow;shift.EndMileage=mileage;
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return shift;
    }
    public Task<List<Maintenance>> Maintenance(long carId,CancellationToken ct)=>db.MaintenanceRecords.AsNoTracking().Where(x=>x.CarId==carId).OrderByDescending(x=>x.ServiceDate).ToListAsync(ct);
    public Task<Maintenance> MaintenanceById(long id,CancellationToken ct)=>Find<Maintenance>(id,ct);
    private static void CheckMaintenance(Maintenance m)
    {if(m.Cost<0 || m.MileageAtService<0 || string.IsNullOrWhiteSpace(m.Description) || m.ServiceDate==default)throw Bad("INVALID_MAINTENANCE","Invalid maintenance entry.");}
    public async Task<Maintenance> AddMaintenance(long carId,Maintenance m,CancellationToken ct,long actorId)
    {if(!await db.Cars.AnyAsync(x=>x.Id==carId,ct))throw Missing();CheckMaintenance(m);m.Id=0;m.CarId=carId;m.CreatedAt=DateTime.UtcNow;db.MaintenanceRecords.Add(m);await db.SaveChangesAsync(ct);RecordAudit(actorId,"MAINTENANCE_CREATED","maintenance",m.Id);await db.SaveChangesAsync(ct);return m;}
    public async Task<Maintenance> UpdateMaintenance(long id,Maintenance input,CancellationToken ct,long actorId)
    {CheckMaintenance(input);var m=await Find<Maintenance>(id,ct);m.Description=input.Description;m.Cost=input.Cost;m.ServiceDate=input.ServiceDate;m.MileageAtService=input.MileageAtService;RecordAudit(actorId,"MAINTENANCE_UPDATED","maintenance",id);await db.SaveChangesAsync(ct);return m;}
    public Task<List<EnergyLog>> EnergyLogs(long driverId,CancellationToken ct)=>db.EnergyLogs.AsNoTracking().Where(x=>db.Shifts.Any(s=>s.Id==x.ShiftId && s.DriverId==driverId)).OrderByDescending(x=>x.CreatedAt).ToListAsync(ct);
    public async Task<EnergyLog> AddEnergy(long driverId,EnergyLog log,CancellationToken ct)
    {
        var shift=await db.Shifts.FirstOrDefaultAsync(x=>x.DriverId==driverId && x.Status=="open",ct)??throw Conflict("OPEN_SHIFT_REQUIRED","Open shift required.");
        if(log.Quantity<=0||log.TotalCost<0||string.IsNullOrWhiteSpace(log.StationName)||log.Unit is not ("liter" or "kwh")||log.OdometerKm<0)throw Bad("INVALID_ENERGY_LOG","Invalid refueling entry.");
        var fuel=await (from car in db.Cars join model in db.CarModels on car.ModelId equals model.Id where car.Id==shift.CarId select model.FuelType).FirstAsync(ct);
        if((fuel=="electric" && log.Unit!="kwh")||(fuel!="electric" && log.Unit!="liter"))throw Conflict("WRONG_ENERGY_UNIT","Energy unit does not match car fuel type.");
        log.Id=0;log.ShiftId=shift.Id;log.CreatedAt=DateTime.UtcNow;db.EnergyLogs.Add(log);await db.SaveChangesAsync(ct);return log;
    }
    public Task<List<Violation>> Violations(long driverId,CancellationToken ct)=>db.Violations.AsNoTracking().Where(x=>x.DriverId==driverId).OrderByDescending(x=>x.ViolationDate).ToListAsync(ct);
    public async Task<Violation> AddViolation(long driverId,long adminId,Violation v,CancellationToken ct)
    {
        if(!await db.DriverProfiles.AnyAsync(x=>x.UserId==driverId,ct))throw Missing();
        if(v.FineAmount<0||string.IsNullOrWhiteSpace(v.Description)||v.ViolationDate==default)throw Bad("INVALID_VIOLATION","Invalid violation.");
        v.Id=0;v.DriverId=driverId;v.CreatedByAdminId=adminId;v.CreatedAt=DateTime.UtcNow;db.Violations.Add(v);await db.SaveChangesAsync(ct);RecordAudit(adminId,"VIOLATION_CREATED","violation",v.Id,new { driver_id=driverId });await db.SaveChangesAsync(ct);return v;
    }
    public async Task<object> Dashboard(CancellationToken ct)
    {
        var today=DateTime.UtcNow.Date;var tomorrow=today.AddDays(1);
        return new{active_drivers=await db.Users.CountAsync(x=>x.Role==UserRole.Driver&&x.Status==UserStatus.Active,ct),open_shifts=await db.Shifts.CountAsync(x=>x.Status=="open",ct),active_cars=await db.Cars.CountAsync(x=>x.Status=="active",ct),orders_today=await db.Orders.CountAsync(x=>x.CreatedAt>=today&&x.CreatedAt<tomorrow,ct),completed_today=await db.Orders.CountAsync(x=>x.CompletedAt>=today&&x.CompletedAt<tomorrow,ct),revenue_today=await db.Payments.Where(x=>x.PaymentStatus=="succeeded"&&x.CreatedAt>=today&&x.CreatedAt<tomorrow).SumAsync(x=>(decimal?)x.Amount,ct)??0m};
    }
    public Task<List<Payment>> Payments(CancellationToken ct)=>db.Payments.AsNoTracking().OrderByDescending(x=>x.CreatedAt).ToListAsync(ct);
    public Task<Payment> Payment(long id,CancellationToken ct)=>Find<Payment>(id,ct);
    public async Task<List<Payment>> TripPayments(long tripId,long userId,bool admin,CancellationToken ct)
    {
        await VerifyTrip(tripId,userId,admin,ct);
        return await db.Payments.AsNoTracking().Where(x=>x.TripId==tripId).OrderByDescending(x=>x.Id).ToListAsync(ct);
    }
    private async Task<(Trip trip,Order order)> VerifyTrip(long tripId,long userId,bool admin,CancellationToken ct)
    {
        var result=await (from trip in db.Trips join order in db.Orders on trip.OrderId equals order.Id where trip.Id==tripId select new {trip,order}).FirstOrDefaultAsync(ct);
        if(result is null || (!admin && result.order.ClientId!=userId && result.order.AssignedDriverId!=userId))throw Missing();
        return(result.trip,result.order);
    }
    public async Task<Payment> CreatePayment(long tripId,long userId,string method,string? key,CancellationToken ct)
    {
        var (trip,order)=await VerifyTrip(tripId,userId,false,ct);
        if(order.ClientId!=userId)throw Missing();
        if(trip.FinalFare is null||trip.ActualEndTime is null)throw Conflict("TRIP_NOT_COMPLETED","Trip is not complete.");
        if(method is not ("cash" or "card" or "wallet"))throw Bad("INVALID_PAYMENT_METHOD","Unknown payment method.");
        await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
        if(!string.IsNullOrWhiteSpace(key))
        {
            var repeat=await db.Payments.FirstOrDefaultAsync(x=>x.IdempotencyKey==key,ct);
            if(repeat is not null){if(repeat.TripId!=tripId||repeat.PaymentMethod!=method)throw Conflict("IDEMPOTENCY_KEY_REUSED","Idempotency key belongs to another request.");return repeat;}
        }
        if(await db.Payments.AnyAsync(x=>x.TripId==tripId&&x.PaymentStatus=="succeeded",ct))throw Conflict("ALREADY_PAID","Trip already paid.");
        // Mock payments: cash succeeds immediately; electronic payments need a configured provider.
        var p=new Payment{TripId=tripId,Amount=trip.FinalFare.Value,Currency="UAH",PaymentMethod=method,PaymentStatus=method=="cash"?"succeeded":"pending",Provider=method=="cash"?"cash":null,IdempotencyKey=key};
        db.Payments.Add(p);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return p;
    }
    public async Task<List<Review>> Reviews(long tripId,long userId,bool admin,CancellationToken ct)
    {await VerifyTrip(tripId,userId,admin,ct);return await db.Reviews.AsNoTracking().Where(x=>x.TripId==tripId).ToListAsync(ct);}
    public async Task<Review> AddReview(long tripId,long userId,byte rating,string? comment,CancellationToken ct)
    {
        if(rating is <1 or >5)throw Bad("INVALID_RATING","Rating must be between 1 and 5.");
        await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
        var (trip,order)=await VerifyTrip(tripId,userId,false,ct);
        if(trip.ActualEndTime is null||order.Status!=OrderStatus.Completed)throw Conflict("TRIP_NOT_COMPLETED","Only completed trip can be reviewed.");
        if(await db.Reviews.AnyAsync(x=>x.TripId==tripId&&x.ReviewerId==userId,ct))throw Conflict("REVIEW_ALREADY_EXISTS","You already reviewed this trip.");
        var target=order.ClientId==userId?order.AssignedDriverId:order.ClientId;
        if(target is null)throw Conflict("DRIVER_MISSING","No driver assigned.");
        var review=new Review{TripId=tripId,ReviewerId=userId,TargetUserId=target.Value,Rating=rating,Comment=comment};
        db.Reviews.Add(review);
        if(order.ClientId==userId)
        {
            var driver=await db.DriverProfiles.FirstAsync(x=>x.UserId==target,ct);
            driver.RatingAverage=Math.Round((driver.RatingAverage*driver.RatingCount+rating)/(driver.RatingCount+1),2);
            driver.RatingCount++;
        }
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return review;
    }
    public Task<List<Trip>> DriverTrips(long driverId,CancellationToken ct) =>
        db.Trips.AsNoTracking().Where(x=>db.Orders.Any(o=>o.Id==x.OrderId && o.AssignedDriverId==driverId)).OrderByDescending(x=>x.CreatedAt).ToListAsync(ct);
    public async Task<Trip> DriverTrip(long driverId,long tripId,CancellationToken ct)
    {
        var trip=await db.Trips.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==tripId && db.Orders.Any(o=>o.Id==x.OrderId && o.AssignedDriverId==driverId),ct);
        return trip??throw Missing();
    }
    public Task<List<Trip>> DriverTripsAdmin(long driverId,CancellationToken ct)=>DriverTrips(driverId,ct);
    public Task<List<Shift>> DriverShiftsAdmin(long driverId,CancellationToken ct)=>DriverShifts(driverId,ct);
    public async Task<object> RevenueReport(DateTime from,DateTime to,CancellationToken ct)
    {
        if(from>to)throw Bad("INVALID_DATE_RANGE","From must precede to.");
        var values=await db.Payments.AsNoTracking().Where(x=>x.PaymentStatus=="succeeded" && x.CreatedAt>=from && x.CreatedAt<to).GroupBy(x=>x.PaymentMethod).Select(g=>new {payment_method=g.Key, count=g.Count(),amount=g.Sum(x=>x.Amount)}).ToListAsync(ct);
        return new{from,to,data=values};
    }
    public async Task<object> TripsReport(DateTime from,DateTime to,CancellationToken ct)
    {
        if(from>to)throw Bad("INVALID_DATE_RANGE","From must precede to.");
        var statuses=await db.Orders.AsNoTracking().Where(x=>x.CreatedAt>=from&&x.CreatedAt<to).GroupBy(x=>x.Status).Select(g=>new {status=g.Key,count=g.Count()}).ToListAsync(ct);
        return new{from,to,data=statuses};
    }
    public async Task<object> DriversReport(CancellationToken ct)
    {
        return await db.DriverProfiles.AsNoTracking().Select(x=>new {driver_id=x.UserId,x.RatingAverage,x.RatingCount,completed_trips=db.Trips.Count(t=>db.Orders.Any(o=>o.Id==t.OrderId && o.AssignedDriverId==x.UserId && o.Status==OrderStatus.Completed))}).OrderByDescending(x=>x.completed_trips).ToListAsync(ct);
    }
    public async Task<object> CarsReport(CancellationToken ct)
    {
        return await db.Cars.AsNoTracking().Select(x=>new {car_id=x.Id,x.LicensePlate,x.Status,total_shifts=db.Shifts.Count(s=>s.CarId==x.Id),total_trips=db.Trips.Count(t=>db.Shifts.Any(s=>s.Id==t.ShiftId&&s.CarId==x.Id))}).ToListAsync(ct);
    }

}
