using System;

[Serializable]
public class Pair<T,K>
{
    public T a;
    public K b;
    public Pair() { }
    public Pair(T a, K b) {
        this.a = a;
        this.b = b;
    }
}
