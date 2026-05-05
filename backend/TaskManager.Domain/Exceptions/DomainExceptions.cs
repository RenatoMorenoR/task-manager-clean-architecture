namespace TaskManager.Domain.Exceptions;

public class DomainValidationException(string message) : Exception(message);

public class UnauthorizedTaskAccessException(Guid taskId, Guid userId) 
    : Exception($"User {userId} does not have access to task {taskId}.");

public class NotFoundException(string entityName, object key) 
    : Exception($"{entityName} with key {key} was not found.");

public class ConflictException(string message) : Exception(message);

public class AuthenticationException(string message) : Exception(message);
