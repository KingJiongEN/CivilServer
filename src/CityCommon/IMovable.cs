public interface IMovable
{
    public int Guid { get; }
    public MyVector2 Position { get; set; }
    public bool IsDead { get; }
    public void OnPause();
    public void OnResume();
    public void OnMove(MyVector2 delta);
    public void OnMoveStart(MyVector2 delta);
    public void OnIdle();
}