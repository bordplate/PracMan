namespace TrainManCore.Scripting.UI;

public interface ITrainer {
    public IWindow CreateWindow();
    void Alert(string text);
}