namespace VoiceProcessor.Engines.Contracts;

public interface IPasswordEngine
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
