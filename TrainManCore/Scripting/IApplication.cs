namespace TrainManCore.Scripting;

public interface IApplication {
    public void OnModuleLoad(Module module, Target.Target target);
    public void OpenModLoader(Target.Target target);
    public void RunOnMainThread(Action action);
    public void Alert(string title, string message);
    public void ConfirmDialog(string title, string message, Action<bool> callback);
    public string LoadFileFromDialog();
}