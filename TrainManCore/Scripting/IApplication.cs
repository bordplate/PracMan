namespace TrainManCore.Scripting;

public interface IApplication {
    public void OnModuleLoad(Module module, Target.Target target);
    public void OpenModLoader(Target.Target target);
    public void RunOnMainThread(Action action);
}