using System;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
namespace MediaPortal.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIDialogProgress : GUIWindow
	{
    const int CONTROL_PROGRESS_BAR =20;

    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
    
    bool m_bCanceled=false;
    bool m_bOverlay=false;
    
    public GUIDialogProgress()
    {
      GetID=(int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\dialogProgress.xml");
    }

    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    
    public override void PreInit()
    {
      AllocResources();
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    #region Base Dialog Members
    public void RenderDlg()
    {
      // render the parent window
      if (null!=m_pParentWindow) 
        m_pParentWindow.Render();

      GUIFontManager.Present();
      // render this dialog box
      base.Render();
    }

    public void Close()
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      m_pParentWindow=null;
      m_bRunning=false;
      GUIGraphicsContext.Overlay=m_bOverlay;
    }
    
    public void StartModal(int dwParentId)
    {
      m_bCanceled=false;
      m_dwParentWindowID=dwParentId;
      m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
      if (null==m_pParentWindow)
      {
        m_dwParentWindowID=0;
        return;
      }

      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);
      ShowProgressBar(false);
      SetPercentage(0);
      m_bRunning=true;
    }

    public void Progress()
    {
      if  (m_bRunning)
      {
        GUIWindowManager.Process();
      }
    }

    public void ProgressKeys()
    {
      if  (m_bRunning)
      {
        //TODO
        //g_application.FrameMove();
      }
    }


    public void DoModal(int dwParentId)
    {
      m_dwParentWindowID=dwParentId;
      m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
      if (null==m_pParentWindow)
      {
        m_dwParentWindowID=0;
        return;
      }

      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
      OnMessage(msg);

      m_bRunning=true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }
    #endregion
	
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          m_pParentWindow=null;
          GUIGraphicsContext.Overlay=m_bOverlay;				
          FreeResources();
          DeInitControls();

          return true;
        }
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:

        {
          m_bOverlay=GUIGraphicsContext.Overlay;
          m_bCanceled = false;
          base.OnMessage(message);
          GUIGraphicsContext.Overlay=false;
          m_pParentWindow=GUIWindowManager.GetWindow(m_dwParentWindowID);
        }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iAction=message.Param1;
          int iControl=message.SenderControlId;
          if (iControl==10)
          {
            m_bCanceled=true;
            return true;
          }
        }
        break;
      }

      if (m_pParentWindow!=null)
      {
        if (message.TargetWindowId==m_pParentWindow.GetID)
        {
          return m_pParentWindow.OnMessage(message);
        }
      }
      return base.OnMessage(message);
    }


    public bool IsCanceled
    {
      get { return m_bCanceled;}
    }

    public void  SetHeading( string strLine)
    {
      SetLine(1,"");
      SetLine(2,"");
      SetLine(3,"");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,2,0,0,null);
      msg.Label=strLine; 
      OnMessage(msg);

    }

    public void SetHeading(int iString)
    {
      SetHeading (GUILocalizeStrings.Get(iString) );
    }

    public void SetLine(int iLine, string strLine)
    {
      if (iLine<1) return;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,2+iLine,0,0,null);
      msg.Label=strLine; 
      OnMessage(msg);
    }

    public void SetLine(int iLine,int iString)
    {
      SetLine (iLine, GUILocalizeStrings.Get(iString) );
    }

    public override void Render()
    {
      RenderDlg();
    }
    
    public void SetPercentage(int iPercentage)
    {
      //TODO
      GUIProgressControl pControl = (GUIProgressControl)GetControl(CONTROL_PROGRESS_BAR);
      if (pControl!=null) pControl.Percentage=iPercentage;
    }

    public void ShowProgressBar(bool bOnOff)
    {
      if (bOnOff)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE,GetID,0, CONTROL_PROGRESS_BAR,0,0,null); 
        OnMessage(msg);
    
      }
      else
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN,GetID,0, CONTROL_PROGRESS_BAR,0,0,null); 
        OnMessage(msg);
      }
    }
  }
}

