using System;

namespace Collector
{
    public class Result
    {
        public bool Success { get; }
        public string Error { get; private set; }
        public bool IsFailure => !Success;

        public Result(bool success, string error)
        {
            if (success && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException();
            if (!success && string.IsNullOrEmpty(error))
                throw new InvalidOperationException();

            Success = success;
            Error = error;
        }

        public static Result Fail(string message) => new Result(false, message);
        public static Result Ok() => new Result(true, string.Empty);

        public static Result<T> Fail<T>(string message) => new Result<T>(default(T), false, message);
        public static Result<T> Ok<T>(T value) => new Result<T>(value, true, string.Empty);
    }

    public class Result<T> : Result
    {
        public T Value { get; set; }

        public Result(T value, bool success, string error)
            :base(success, error)
        {
            Value = value;
        }
    }
}