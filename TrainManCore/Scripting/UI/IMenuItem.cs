namespace TrainManCore.Scripting.UI;

public interface IMenuItem: IControl {
    public IWindow Window { get; }
    public string Title { get; set; }
    public bool IsCheckable { get; set; }
    public bool Checked { get; set; }
}