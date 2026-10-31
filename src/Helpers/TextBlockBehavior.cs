using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Composition;

namespace BitWallpaper.Helpers;

public static class TextBlockBehavior
{
    public static readonly DependencyProperty BlinkOnChangeProperty = DependencyProperty.RegisterAttached(
            "BlinkOnChange",
            typeof(bool),
            typeof(TextBlockBehavior),
            new PropertyMetadata(false, OnBlinkOnChangeChanged));

    public static bool GetBlinkOnChange(DependencyObject obj) => (bool)obj.GetValue(BlinkOnChangeProperty);
    public static void SetBlinkOnChange(DependencyObject obj, bool value) => obj.SetValue(BlinkOnChangeProperty, value);

    private static void OnBlinkOnChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock textBlock && e.NewValue is bool isEnabled)
        {
            if (isEnabled)
            {
                // Register strictly to this specific TextBlock instance's text changes
                textBlock.RegisterPropertyChangedCallback(TextBlock.TextProperty, OnTextChanged);
            }
            // Note: WinUI garbage collection automatically cleans up property callbacks 
            // when the TextBlock is destroyed, so manual unregistering isn't strictly required here.
        }
    }

    private static void OnTextChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (sender is TextBlock textBlock)
        {
            // Skip running the animation during initial load if the text is empty
            if (string.IsNullOrEmpty(textBlock.Text)) return;

            TriggerBlinkAnimation(textBlock);
        }
    }

    private static void TriggerBlinkAnimation(TextBlock textBlock)
    {
        // Get the underlying visual surface via WinUI Composition API
        Visual visual = ElementCompositionPreview.GetElementVisual(textBlock);
        Compositor compositor = visual.Compositor;

        // Define a quick fade out and fade in animation sequence
        ScalarKeyFrameAnimation animation = compositor.CreateScalarKeyFrameAnimation();
        animation.Duration = TimeSpan.FromMilliseconds(400);

        // KeyFrames: 0% (Fully Visible) -> 50% (Faded to 20%) -> 100% (Fully Visible)
        animation.InsertKeyFrame(0.0f, 1.0f);
        animation.InsertKeyFrame(0.5f, 0.2f);
        animation.InsertKeyFrame(1.0f, 1.0f);

        // Execute the animation directly on the compositor thread
        visual.StartAnimation("Opacity", animation);
    }
}
