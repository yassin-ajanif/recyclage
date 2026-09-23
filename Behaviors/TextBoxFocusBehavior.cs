using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Recyclage.Behaviors;

public static class TextBoxFocusBehavior
{
    public static readonly AttachedProperty<bool> SelectAllOnFocusProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>(
            "SelectAllOnFocus",
            typeof(TextBoxFocusBehavior),
            defaultValue: false);

    static TextBoxFocusBehavior()
    {
        SelectAllOnFocusProperty.Changed.AddClassHandler<TextBox>(OnSelectAllOnFocusChanged);
    }

    public static bool GetSelectAllOnFocus(TextBox element) =>
        element.GetValue(SelectAllOnFocusProperty);

    public static void SetSelectAllOnFocus(TextBox element, bool value) =>
        element.SetValue(SelectAllOnFocusProperty, value);

    private static void OnSelectAllOnFocusChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
    {
        if (Equals(e.NewValue, true))
            AttachHandlers(textBox);
        else
            DetachHandlers(textBox);
    }

    private static void AttachHandlers(TextBox textBox)
    {
        textBox.GotFocus -= OnGotFocus;
        textBox.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        textBox.GotFocus += OnGotFocus;
        textBox.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
    }

    private static void DetachHandlers(TextBox textBox)
    {
        textBox.GotFocus -= OnGotFocus;
        textBox.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
    }

    private static void OnGotFocus(object? sender, GotFocusEventArgs e) =>
        DeferSelectAll(sender as TextBox);

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBox textBox ||
            !GetSelectAllOnFocus(textBox) ||
            !e.GetCurrentPoint(textBox).Properties.IsLeftButtonPressed)
            return;

        if (textBox.IsKeyboardFocusWithin)
            return;

        textBox.Focus();
        DeferSelectAll(textBox);
        e.Handled = true;
    }

    private static void DeferSelectAll(TextBox? textBox)
    {
        if (textBox is null || !GetSelectAllOnFocus(textBox))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (textBox.IsKeyboardFocusWithin)
                textBox.SelectAll();
        }, DispatcherPriority.Input);
    }
}
