namespace HotelListingAPI.Results;

public readonly record struct Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public bool IsNone => string.IsNullOrWhiteSpace(Code);
}

public readonly record struct Result
{
    #region Properties
    public bool IsSuccess { get; }
    public Error[] Errors { get; }
    #endregion

    #region Constructors
    private Result(bool isSuccess, Error[] errors)
        => (IsSuccess, Errors) = (isSuccess, errors);
    #endregion

    #region methods
    // If success, privately assign true to IsSuccess, and nothing to Errors.
    public static Result Success() => new(isSuccess: true, errors: Array.Empty<Error>());
    public static Result Failure(params Error[] errors) => new(isSuccess: false, errors: errors);
    public static Result NotFound(params Error[] errors) => new(isSuccess: false, errors: errors);
    public static Result BadRequest(params Error[] errors) => new(isSuccess: false, errors: errors);

    public static Result Combine(params Result[] results)
        // Check for any results without success, and also results with success
        => results.Any(r => !r.IsSuccess)
        ?
        Failure(results
            .Where(r => !r.IsSuccess)
            .SelectMany(r => r.Errors)
            .ToArray())
        :
        Success();
    #endregion
}

public readonly record struct Result<T>
{
    #region Fields
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error[] Errors { get; }
    #endregion

    #region Constructor
    private Result(bool isSuccess, T? value, Error[] errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }
    #endregion

    #region Methods - Result states
    public static Result<T> Success(T value) => new(isSuccess: true, value: value, errors: []);
    public static Result<T> Failure(params Error[] errors) => new(isSuccess: false, value: default, errors: errors);
    public static Result<T> NotFound(params Error[] errors) => new(false, default, errors);
    public static Result<T> BadRequest(params Error[] errors) => new(false, default, errors);
    #endregion

    #region Functional Helpers
    public Result<K> Map<K>(Func<T, K> map)
        => IsSuccess
        ?
        Result<K>.Success(map(Value!))
        :
        Result<K>.Failure(Errors);

    public Result<K> Bind<K>(Func<T, Result<K>> next)
        => IsSuccess
        ?
        next(Value!)
        :
        Result<K>.Failure(errors: Errors);

    public Result<T> Ensure(Func<T, bool> predicate, Error error)
        => IsSuccess && !predicate(Value!)
        ?
        Failure(error)
        :
        this;

    #endregion
}
