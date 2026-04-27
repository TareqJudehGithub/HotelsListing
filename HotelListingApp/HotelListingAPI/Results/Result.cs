namespace HotelListingAPI.Results;

public readonly record struct ErrorPat
{
    #region Properties
    public string Code { get; }
    public string Description { get; }
    #endregion

    #region Constructor
    public ErrorPat(string code, string description)
    {
        Code = code;
        Description = description;
    }
    #endregion

    #region Methods
    public static readonly Error None = new(string.Empty, string.Empty);
    public bool IsNone => string.IsNullOrWhiteSpace(Code);
    #endregion
}

// Base Result class
public readonly record struct ResultBased
{
    #region properties

    #endregion
    public bool IsSuccess { get; }
    public ErrorPat[] Errors { get; }

    #region Constructor
    public ResultBased(bool isSuccess, ErrorPat[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }
    #endregion

    #region Methods
    // Success
    public static ResultBased Success() => new(isSuccess: true, errors: []);
    // Failure
    public static ResultBased Failure(params ErrorPat[] errors) => new(isSuccess: false, errors: errors);
    // NotFound
    public static ResultBased NotFound(params ErrorPat[] errors) => new(isSuccess: false, errors: errors);
    //BadRequest
    public static ResultBased BadRequest(params ErrorPat[] errors) => new(isSuccess: false, errors: errors);
    #endregion

}
// Type/generic Result class
public readonly record struct ResultTyped<T>
{
    #region Fields
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorPat[] Errors { get; }
    #endregion

    #region Constructor
    private ResultTyped(bool isSuccess, T? value, ErrorPat[] errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }
    #endregion

    #region Methods - Result states
    public static ResultTyped<T> Success(T value) => new(isSuccess: true, value: value, errors: []);
    public static ResultTyped<T> Failure(params ErrorPat[] errors) => new(isSuccess: false, value: default, errors: errors);
    public static ResultTyped<T> NotFound(params ErrorPat[] errors) => new(false, default, errors);
    public static ResultTyped<T> BadRequest(params ErrorPat[] errors) => new(false, default, errors);
    #endregion
}

#region MyRegion

#endregion