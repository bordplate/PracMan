using NLua;

namespace TrainManCore.Scripting.UI;

public interface ITrainer {
    /// <summary>
    /// Creates a new window.
    /// Main windows must be created first and only one main window can be created.
    /// </summary>
    /// <param name="module"></param>
    /// <param name="isMainWindow"></param>
    /// <returns></returns>
    public IWindow CreateWindow(Module module, bool isMainWindow = false);
    public IMenu AddMenu(string title, Action<IMenu> callback);
    public void CloseAllWindows();
    void Alert(string text);
}