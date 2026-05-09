namespace NetSend.Shared.Types;

public class Result<TValue, TError>
{
    public Result(TValue value)
    {
        _isError = false;
        Value = value;
        Error = default!;
    }

    public Result(TError? error, int debugCode = 0)
    {
        _isError = true;
        Error = error!;
        Value = default!;
        DebugCode = debugCode;
    }

    public Result(TError? error)
    {
        _isError = true;
        Error = error!;
        Value = default!;
        DebugCode = 0;
    }

    public int DebugCode { get; } = 0;

    private bool _isError;

    public bool IsError => _isError;
    public bool IsSuccess => !_isError;

    public TValue Value { get; }
    public TError Error { get; }

    public static Result<TValue, TError> AsError(TError error) => new(error: error);
    public static Result<TValue, TError> AsValue(TValue value) => new(value: value);
}

public class VoidResult<TError>
{
    /// <summary>
    /// Returns success result
    /// </summary>
    public VoidResult()
    {
        _isError = false;
    }

    public VoidResult(TError? error, int debugCode = 0)
    {
        _error = error;
        _isError = true;
        DebugCode = debugCode;
    }

    public int DebugCode { get; } = 0;

    private readonly bool _isError;
    public bool IsError => _isError;
    public bool IsSuccess => !_isError;

    private readonly TError? _error = default;
    public TError Error => _error!;

    public static VoidResult<TError> Success { get; } = new();
}

public class ValueResult<TValue>
{
    public ValueResult(int debugCode = 0)
    {
        _isError = true;
        DebugCode = debugCode;
    }

    public ValueResult(TValue value)
    {
        _value = value;
        _isError = false;
    }

    public int DebugCode { get; } = 0;

    private readonly bool _isError;
    public bool IsError => _isError;
    public bool IsSuccess => !_isError;

    private readonly TValue? _value = default;
    public TValue Value => _value!;

    public static ValueResult<TValue> Error { get; } = new();
}