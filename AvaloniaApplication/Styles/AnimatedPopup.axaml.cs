using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace AvaloniaApplication;

public class AnimatedPopup : ContentControl
{
    #region Private Members

    /// <summary>
    /// Store the controls desired size
    /// </summary>
    private Size mDesiredSize;
    /// <summary>
    /// A flag for when we are animating
    /// </summary>
    private bool mAnimating;
    /// <summary>
    /// The animation UI timer
    /// </summary>
    private DispatcherTimer mAnimateTimer;
    /// <summary>
    /// The current position in the animation
    /// </summary>
    private int mCurrentAnimationTick;
    #endregion
    
    #region Constructor
    /// <summary>
    /// Default Constructor
    /// </summary>
    public AnimatedPopup()
    {
        //Get a 60 FPS timespan
       var framerate = TimeSpan.FromSeconds(1/60.0);
       
       //Make a new dispatcher timer
       mAnimateTimer = new DispatcherTimer
       {
           //Set the timer to run 60 times a second 
           Interval = framerate
       };
       
       //Fix for 3 seconds
       var animation = TimeSpan.FromSeconds(1);
       
       //Calculate total ticks that make up the animation time
       var totalTicks = (int)(animation.TotalSeconds / framerate.TotalSeconds);
       
       //Keep track of current tick
       mCurrentAnimationTick = 0;

       //Callback on every tick
       mAnimateTimer.Tick += (s, e) =>
       {
           //Increment the tick
           mCurrentAnimationTick++;
           
           //Set animating flag
           mAnimating = true;
           
           //If we have reached total ticks...
           if (mCurrentAnimationTick > totalTicks)
           {
               //Stop this animation timer
               mAnimateTimer.Stop();
               
               //Clear animating flag
               mAnimating = false;
               
               //Break out of code
               return;
           }
           
           //Get percentage of the way through the current animation
           var percentageAnimated = (float)mCurrentAnimationTick / totalTicks;
           
           //Make an animation easing
           var easing = new QuadraticEaseIn();
           
           //Calculate final width and height
           var finalWidth = mDesiredSize.Width * easing.Ease(percentageAnimated);
           var finalHeight = mDesiredSize.Height * easing.Ease(percentageAnimated);
           
           //Do our animation
           Width = finalWidth;
           Height = finalHeight;

           
           
           Console.WriteLine($"Current tick: {mCurrentAnimationTick}");
       };
       
    }
    
    #endregion

    public override void Render(DrawingContext context)
    {
        //if we are not animating...
        if (!mAnimating)
        {
            //Update the new desired size only once
            
            //Set desired size (which includes the margin, so remove that from our calculation)
            mDesiredSize = DesiredSize - Margin;

            //Reset animation position
            mCurrentAnimationTick = 0;
                
            //Start timer
            mAnimateTimer.Start();
            Console.WriteLine($"Desired size: {mDesiredSize}");
        }
        base.Render(context);
    }
}