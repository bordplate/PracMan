using ObjCRuntime;

namespace TrainMan;

public partial class ViewController : NSViewController
{
    // NSView mainContentView;
    // NSButton addStackViewButton;
    // NSStackView stackViewsContainer;
    //
    // protected ViewController(NativeHandle handle) : base(handle)
    // {
    //     Console.WriteLine("What");
    //     // This constructor is required if the view controller is loaded from a xib or a storyboard.
    //     // Do not put any initialization here, use ViewDidLoad instead.
    // }
    //
    // public override void ViewDidLoad() {
    //     base.ViewDidLoad();
    //     
    //     // Set up the main content view
    //         mainContentView = new NSView
    //         {
    //             TranslatesAutoresizingMaskIntoConstraints = false
    //         };
    //         View.AddSubview(mainContentView);
    //
    //         // Constraints to make mainContentView fill the window
    //         NSLayoutConstraint.ActivateConstraints(new[]
    //         {
    //             mainContentView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
    //             mainContentView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
    //             mainContentView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
    //             mainContentView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
    //         });
    //
    //         // Create the button at the bottom of the mainContentView
    //         addStackViewButton = new NSButton
    //         {
    //             Title = "Add Stack View",
    //             BezelStyle = NSBezelStyle.Rounded,
    //             TranslatesAutoresizingMaskIntoConstraints = false
    //         };
    //         addStackViewButton.Activated += AddStackViewButton_Activated;
    //
    //         mainContentView.AddSubview(addStackViewButton);
    //
    //         // Constraints for the button
    //         NSLayoutConstraint.ActivateConstraints(new[]
    //         {
    //             addStackViewButton.BottomAnchor.ConstraintEqualTo(mainContentView.BottomAnchor, -20),
    //             addStackViewButton.CenterXAnchor.ConstraintEqualTo(mainContentView.CenterXAnchor),
    //         });
    //
    //         // Create a container stack view for the dynamic stack views
    //         stackViewsContainer = new NSStackView
    //         {
    //             Orientation = NSUserInterfaceLayoutOrientation.Vertical,
    //             Alignment = NSLayoutAttribute.Top,
    //             Spacing = 10,
    //             TranslatesAutoresizingMaskIntoConstraints = false,
    //             Distribution = NSStackViewDistribution.FillEqually
    //         };
    //         mainContentView.AddSubview(stackViewsContainer);
    //
    //         // Constraints for the stackViewsContainer
    //         NSLayoutConstraint.ActivateConstraints(new[]
    //         {
    //             stackViewsContainer.LeadingAnchor.ConstraintEqualTo(mainContentView.LeadingAnchor, 20),
    //             stackViewsContainer.TrailingAnchor.ConstraintEqualTo(mainContentView.TrailingAnchor, -20),
    //             stackViewsContainer.TopAnchor.ConstraintEqualTo(mainContentView.TopAnchor, 20),
    //             stackViewsContainer.BottomAnchor.ConstraintLessThanOrEqualTo(addStackViewButton.TopAnchor, -20),
    //         });
    //     }
    //
    //     private void AddStackViewButton_Activated(object sender, EventArgs e)
    //     {
    //         // Create a new vertical stack view
    //         var newStackView = new NSStackView
    //         {
    //             Orientation = NSUserInterfaceLayoutOrientation.Vertical,
    //             Alignment = NSLayoutAttribute.Trailing,
    //             Spacing = 5,
    //             TranslatesAutoresizingMaskIntoConstraints = false,
    //             Distribution = NSStackViewDistribution.Fill
    //         };
    //
    //         // Create the button to add text fields
    //         var addTextFieldButton = new NSButton
    //         {
    //             Title = "Add Text Field",
    //             BezelStyle = NSBezelStyle.Rounded,
    //             TranslatesAutoresizingMaskIntoConstraints = false
    //         };
    //
    //         addTextFieldButton.Activated += (s, args) =>
    //         {
    //             // Create a new text field
    //             var textField = new NSTextField
    //             {
    //                 PlaceholderString = "Enter text",
    //                 TranslatesAutoresizingMaskIntoConstraints = false
    //             };
    //
    //             // Insert the text field before the button in the arranged subviews
    //             var buttonIndex = Array.IndexOf(newStackView.ArrangedSubviews, addTextFieldButton);
    //             newStackView.InsertArrangedSubview(textField, buttonIndex);
    //
    //             // Optionally, set constraints or properties on the text field
    //         };
    //
    //         // Add the button to the stack view
    //         newStackView.AddArrangedSubview(addTextFieldButton);
    //
    //         // Optionally, add a border or background color to distinguish the stack views
    //         // newStackView.WantsLayer = true;
    //         // newStackView.Layer.BackgroundColor = NSColor.LightGray.CGColor;
    //
    //         // Add the new stack view to the container
    //         stackViewsContainer.AddArrangedSubview(newStackView);
    //     }
    //
    // public override NSObject RepresentedObject
    // {
    //     get => base.RepresentedObject;
    //     set
    //     {
    //         base.RepresentedObject = value;
    //
    //         // Update the view, if already loaded.
    //     }
    // }
}