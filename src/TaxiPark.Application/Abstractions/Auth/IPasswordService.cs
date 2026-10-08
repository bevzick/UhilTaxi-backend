using TaxiPark.Domain.Entities;
namespace TaxiPark.Application.Abstractions.Auth;
public interface IPasswordService { string Hash(User user, string password); bool Verify(User user, string password); }
