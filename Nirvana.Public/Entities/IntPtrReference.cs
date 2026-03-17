namespace Nirvana.Public.Entities;

public class IntPtrReference(int initialValue = 100) {
    public int Value { get; set; } = initialValue;
}