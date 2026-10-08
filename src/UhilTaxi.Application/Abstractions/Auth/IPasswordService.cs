using UhilTaxi.Domain.Entities;
namespace UhilTaxi.Application.Abstractions.Auth;
public interface IPasswordService { string Hash(User user, string password); bool Verify(User user, string password); }
