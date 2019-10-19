using System;

using System.Drawing;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.Rendering;

namespace RectBlendEffect
{

  public enum BlendDirection
  {
    BR_TL,
    BL_TR,
    TL_BR,
    TR_BL
  }


  public class BlendEffectConfigToken : EffectConfigToken
  {

    public BlendEffectConfigToken(BlendDirection direction)
    {
      blendDirection = direction;
    }

    public override object Clone()
    {
      return new BlendEffectConfigToken(blendDirection);
    }

    public BlendDirection blendDirection;

  }


  public class EffectDialogImpl : EffectConfigDialog
  {
    System.Windows.Forms.RadioButton b1;
    System.Windows.Forms.RadioButton b2;
    System.Windows.Forms.RadioButton b3;
    System.Windows.Forms.RadioButton b4;
    public EffectDialogImpl()
    {
      System.Windows.Forms.TableLayoutPanel box = new System.Windows.Forms.TableLayoutPanel();

      b1 = new System.Windows.Forms.RadioButton();
      b2 = new System.Windows.Forms.RadioButton();
      b3 = new System.Windows.Forms.RadioButton();
      b4 = new System.Windows.Forms.RadioButton();
      b1.Text = "TL => BR";
      b2.Text = "TR => BL";
      b3.Text = "BR => TL";
      b4.Text = "BL => TR";
      b1.CheckedChanged += new EventHandler(OnRadioClick);
      b2.CheckedChanged += new EventHandler(OnRadioClick);
      b3.CheckedChanged += new EventHandler(OnRadioClick);
      b4.CheckedChanged += new EventHandler(OnRadioClick);
      b3.Checked = true;
      box.Controls.Add(b1);
      box.Controls.Add(b2);
      box.Controls.Add(b3);
      box.Controls.Add(b4);
      box.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      box.AutoSize = true;


      System.Windows.Forms.FlowLayoutPanel upper = new System.Windows.Forms.FlowLayoutPanel();
      upper.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      upper.AutoSize = true;

      System.Windows.Forms.Button go = new System.Windows.Forms.Button();
      go.Text = "Finish";
      go.Click += new EventHandler(finishPressed);

      upper.Controls.Add(box);
      upper.Controls.Add(go);
      Controls.Add(upper);
      this.Text = "Rectangular Corner Fade";
    }
    protected override void InitialInitToken()
    {
      theEffectToken = new BlendEffectConfigToken(BlendDirection.BL_TR);
    }

    protected override void InitTokenFromDialog()
    {
      BlendEffectConfigToken t = theEffectToken as BlendEffectConfigToken;
      if (b1.Checked)
      {
        t.blendDirection = BlendDirection.TL_BR;
      }
      else if (b2.Checked)
      {
        t.blendDirection = BlendDirection.TR_BL;
      }
      else if (b3.Checked)
      {
        t.blendDirection = BlendDirection.BR_TL;
      }
      else if (b4.Checked)
      {
        t.blendDirection = BlendDirection.BL_TR;
      }
    }

    public void finishPressed(object sender, EventArgs args)
    {
      this.DialogResult = System.Windows.Forms.DialogResult.OK;
      FinishTokenUpdate();
      Close();
    }

    public void OnRadioClick(object sender, EventArgs args)
    {
      BlendEffectConfigToken t = theEffectToken as BlendEffectConfigToken;
      switch ((sender as System.Windows.Forms.RadioButton).Text)
      {
        case "TL => BR":
          t.blendDirection = BlendDirection.TL_BR;
          break;
        case "TR => BL":
          t.blendDirection = BlendDirection.TR_BL;
          break;

        case "BL => TR":
          t.blendDirection = BlendDirection.BL_TR;
          break;
        case "BR => TL":
          t.blendDirection = BlendDirection.BR_TL;
          break;
      }
      FinishTokenUpdate();
    }

  }




  public class PluginSupportInfo : IPluginSupportInfo
  {
    public string Author
    {
      get
      {
        return "bluebuiy";
      }
    }
    public string Copyright
    {
      get
      {
        return "";
      }
    }

    public string DisplayName
    {
      get
      {
        return "Rectangular Corner Fade";
      }
    }

    public Version Version
    {
      get
      {
        return new System.Version(1, 0);
      }
    }

    public Uri WebsiteUri
    {
      get
      {
        return new Uri("https://www.getpaint.net/redirect/plugins.html");
      }
    }
  }

  [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Rectangular Corner Fade")]
  public class BlendEffect : PaintDotNet.Effects.Effect
  {



    Rectangle bounds;


    EffectDialogImpl dialog;


    public BlendEffect() :
      base("Rect Corner Fade", null, "Selection", new EffectOptions() { Flags = EffectFlags.Configurable })
    {


    }

    /*
     *  Solves the system [a] + [b]s =  [u] + [v]t
     *  [a - u] = [v]t - [b]s
     */
    private static void SolveLinearSystem2(Point2Float a, Vector2Float b, out float s, Point2Float u, Vector2Float v, out float t)
    {
      Vector2Float constants = a - u;
      double det_denom = Vector2Float.Determinant(v, -b);
      double det_v = Vector2Float.Determinant(constants, -b);
      double det_b = Vector2Float.Determinant(v, constants);

      t = (float)(det_v / det_denom);
      s = (float)(det_b / det_denom);
    }

    public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
    {
      BlendEffectConfigToken token = parameters as BlendEffectConfigToken;
      Point2Float start = new Point2Float();
      Point2Float end = new Point2Float();
      Point2Float p;
      Point2Float q;

      if (token.blendDirection == BlendDirection.BL_TR)
      {
        start = new Point2Float(bounds.Left - 0.5f, bounds.Bottom + 0.5f);
        end = new Point2Float(bounds.Right + 0.5f, bounds.Top - 0.5f);
      }
      else if (token.blendDirection == BlendDirection.BR_TL)
      {
        start = new Point2Float(bounds.Right, bounds.Bottom);
        end = new Point2Float(bounds.Left, bounds.Top);
      }
      else if (token.blendDirection == BlendDirection.TL_BR)
      {
        start = new Point2Float(bounds.Left, bounds.Top);
        end = new Point2Float(bounds.Right, bounds.Bottom);
      }
      else if (token.blendDirection == BlendDirection.TR_BL)
      {
        start = new Point2Float(bounds.Right, bounds.Top);
        end = new Point2Float(bounds.Left, bounds.Bottom);
      }

      if (token.blendDirection == BlendDirection.BL_TR || token.blendDirection == BlendDirection.TR_BL)
      {
        p = new Point2Float(bounds.Right, bounds.Bottom);
        q = new Point2Float(bounds.Left, bounds.Top);
      }
      else
      {
        p = new Point2Float(bounds.Left, bounds.Bottom);
        q = new Point2Float(bounds.Right, bounds.Top);
      }

      Vector2Float lineDir = Vector2Float.Normalize(p - q);

      for (int i = startIndex; i < length; ++i)
      {
        Rectangle rect = rois[i];
        for (int y = rect.Top; y < rect.Bottom; ++y)
        {
          for (int x = rect.Left; x < rect.Right; ++x)
          {
            Point2Float current = new Point2Float(x, y) + new Vector2Float(0.5f, 0.5f);
            Vector2Float dir = Vector2Float.Normalize(end - current);
            float distToCross = 0;
            float dist_ignore = 0;
            SolveLinearSystem2(current, dir, out distToCross, p, Vector2Float.Normalize(q - p), out dist_ignore);

            Point2Float center = new Point2Float(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
            Vector2Float distToCenter = current - center;
            distToCenter.X /= bounds.Width / 2;
            distToCenter.Y /= bounds.Height / 2;
            Point2Float from2;

            if (Math.Abs(distToCenter.X) > Math.Abs(distToCenter.Y))
            {
              from2 = new Point2Float(start.X, end.Y);
            }
            else
            {
              from2 = new Point2Float(end.X, start.Y);
            }

            float distToStart = 0;
            float dist_ignore2 = 0;
            SolveLinearSystem2(current, -dir, out distToStart, from2, Vector2Float.Normalize(from2 - start), out dist_ignore2);


            if (distToCross > 0)
            {
              float t = distToStart / (distToStart + distToCross);

              ColorBgra pixle = srcArgs.Surface.GetPoint(x, y);
              pixle.A = (byte)(t * pixle.A);
              //pixle.R = (byte)(255 * t);
              //pixle.G = 0; // (byte)(from2.Y);
              //pixle.B = 0;
              dstArgs.Surface.SetPoint(x, y, pixle);
            }
            else
            {
              dstArgs.Surface.SetPoint(x, y, srcArgs.Surface.GetPoint(x, y));
            }
          }
        }
      }
    }

    protected override void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
    {
      base.OnSetRenderInfo(parameters, dstArgs, srcArgs);
    
      PdnRegion rgn = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds);
      bounds = rgn.GetBoundsInt();
    
    
    
    
    
    }

    public override EffectConfigDialog CreateConfigDialog()
    {
      dialog = new EffectDialogImpl();
      return dialog;
    }
    

  }


}
