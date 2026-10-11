namespace LibrarySystem.Exceptions
{
    // Los servicios lanzan estas excepciones; la capa de presentación las traducirá a códigos HTTP en un solo lugar.
    public class InvalidInputException(string message) : Exception(message);

    public class ConflictException(string message) : Exception(message);

    public class InvalidCredentialsException() : Exception("Invalid credentials.");

    public class NotFoundException(string message) : Exception(message);

    public class ForbiddenException(string message) : Exception(message);
}
