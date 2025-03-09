namespace MiniTwit.Core;

public class AtomicIntegerFile
{
    private readonly string _filePath;
    private readonly int _defaultValue;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _isSet;
    private int _value;

    public AtomicIntegerFile(string filePath, int defaultValue)
    {
        _filePath = filePath;
        _defaultValue = defaultValue;
    }

    public async Task<int> GetAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!_isSet)
            {
                _value = await ReadOrDefaultAsync();
                _isSet = true;
            }
            return _value;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> SetAsync(int value)
    {
        await _lock.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(_filePath, value.ToString());
            _value = value;
            _isSet = true;
            return value;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<int> ReadOrDefaultAsync()
    {
        if (!File.Exists(_filePath))
        {
            return _defaultValue;
        }
        var text = await File.ReadAllTextAsync(_filePath);
        if (!int.TryParse(text, out var value))
        {
            return _defaultValue;
        }
        return value;
    }
}
