using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Picture.Database;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Pictures
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIPictures: GUIWindow, IComparer, ISetupForm
  {
    [Serializable]
    public class MapSettings
    {
      protected int   _SortBy;
      protected int   _ViewAs;
      protected bool _SortAscending ;

      public MapSettings()
      {
				// Set default view
        _SortBy= (int)SortMethod.SORT_NAME;
				_ViewAs = (int)View.VIEW_AS_ICONS;
        _SortAscending=true;
      }


      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy;}
        set { _SortBy=value;}
      }
      
      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs;}
        set { _ViewAs=value;}
      }
      
      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending;}
        set { _SortAscending=value;}
      }
    }

    enum Controls
    {
      CONTROL_BTNVIEWASICONS=		2,
      CONTROL_BTNSORTBY		=			3,
      CONTROL_BTNSORTASC	=			4,
      CONTROL_BTNSLIDESHOW    =      6,
      CONTROL_BTNSLIDESHOW_RECURSIVE      =       7,
      CONTROL_BTNCREATETHUMBS    =       8,
      CONTROL_BTNROTATE   =      9,
      CONTROL_VIEW      	=			10,
      CONTROL_LABELFILES  =       12
    };
    #region Base variabeles
    enum SortMethod
    {
      SORT_NAME=0,
      SORT_DATE=1,
      SORT_SIZE=2
    }

    enum View
    {
      VIEW_AS_LIST    =       0,
      VIEW_AS_ICONS    =      1,
      VIEW_AS_LARGEICONS  =   2,
      VIEW_AS_FILMSTRIP   =   3,
    }


    const string      ThumbsFolder=@"Thumbs\Pictures";
    int               m_iItemSelected=-1;
		GUIListItem				m_itemItemSelected=null;
    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory="";
		string						m_strDestination="";
    VirtualDirectory  m_directory = new VirtualDirectory();
    MapSettings       _MapSettings = new MapSettings();
		bool							m_bFileMenuEnabled=false;
		string						m_strFileMenuPinCode="";
		    
    #endregion
    

    public GUIPictures()
    {
      GetID=(int)GUIWindow.Window.WINDOW_PICTURES;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.PictureExtensions);
    }
    ~GUIPictures()
    {
      SaveSettings();
    }

    public override bool Init()
    {
      m_strDirectory="";
			m_strDestination="";
			try
			{
      System.IO.Directory.CreateDirectory(ThumbsFolder);
			}
			catch(Exception){}
			bool result= Load (GUIGraphicsContext.Skin+@"\mypics.xml");
			LoadSettings();
			return result;
    }

    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
				m_bFileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
				m_strFileMenuPinCode = xmlreader.GetValueAsString("filemenu", "pincode", "");
        string strDefault=xmlreader.GetValueAsString("pictures", "default","");
        m_directory.Clear();
        for (int i=0; i < 20; i++)
        {
          string strShareName=String.Format("sharename{0}",i);
          string strSharePath=String.Format("sharepath{0}",i);
          string strPincode = String.Format("pincode{0}",i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd  = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);

          Share share=new Share();
          share.Name=xmlreader.GetValueAsString("pictures", strShareName,"");
          share.Path=xmlreader.GetValueAsString("pictures", strSharePath,"");
          share.Pincode = xmlreader.GetValueAsInt("pictures", strPincode, - 1);
          
          share.IsFtpShare= xmlreader.GetValueAsBool("pictures", shareType, false);
          share.FtpServer= xmlreader.GetValueAsString("pictures", shareServer,"");
          share.FtpLoginName= xmlreader.GetValueAsString("pictures", shareLogin,"");
          share.FtpPassword= xmlreader.GetValueAsString("pictures", sharePwd,"");
          share.FtpPort= xmlreader.GetValueAsInt("pictures", sharePort,21);
          share.FtpFolder= xmlreader.GetValueAsString("pictures", remoteFolder,"/");

          if (share.Name.Length>0)
          {

            if (strDefault == share.Name)
            {
              share.Default=true;
              if (m_strDirectory.Length==0) m_strDirectory = share.Path;
            }
            m_directory.Add(share);
          }
          else break;
        }
      }
    }

    void SaveSettings()
    {
    }
    #endregion



    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = GetItem(0);
        if (item!=null)
        {
          if (item.IsFolder && item.Label=="..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        GUIWindowManager.PreviousWindow();
        return;
      }

			if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
			{
				ShowContextMenu();
			}

			if (action.wID == Action.ActionType.ACTION_DELETE_ITEM)
			{
				// delete current picture
				GUIListItem item=GetSelectedItem();
				if (item!=null)
				{
					if (item.IsFolder==false)
					{
						OnDeleteItem(item);
					}
				}
			}		

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
          GUITextureManager.CleanupThumbs();
          LoadSettings();
          LoadFolderSettings(m_strDirectory);
          ShowThumbPanel();
          LoadDirectory(m_strDirectory);
          if (m_iItemSelected>=0)
          {
            GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_VIEW,m_iItemSelected);
          }
										return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          m_iItemSelected=GetSelectedItemNo();
          SaveSettings();          
          SaveFolderSettings(m_strDirectory);
          break;

        case GUIMessage.MessageType.GUI_MSG_START_SLIDESHOW:
        {
          string strUrl = message.Label;
          LoadDirectory( strUrl );
          OnSlideShow();
        }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_BTNVIEWASICONS)
          {
            switch ((View)_MapSettings.ViewAs)
            {
              case View.VIEW_AS_LIST:
                _MapSettings.ViewAs=(int)View.VIEW_AS_ICONS;
                break;
              case View.VIEW_AS_ICONS:
                _MapSettings.ViewAs=(int)View.VIEW_AS_LARGEICONS;
                break;
              case View.VIEW_AS_LARGEICONS:
                _MapSettings.ViewAs=(int)View.VIEW_AS_FILMSTRIP;
                break;
            case View.VIEW_AS_FILMSTRIP:
              _MapSettings.ViewAs=(int)View.VIEW_AS_LIST;
              break;
            }
            ShowThumbPanel();
            GUIControl.FocusControl(GetID,iControl);
          }
          
          if (iControl==(int)Controls.CONTROL_BTNSORTASC)
          {
            _MapSettings.SortAscending=!_MapSettings.SortAscending;
            OnSort();
            GUIControl.FocusControl(GetID,iControl);
          }

          if (iControl==(int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            switch ((SortMethod)_MapSettings.SortBy)
            {
              case SortMethod.SORT_NAME:
                _MapSettings.SortBy=(int)SortMethod.SORT_DATE;
                break;
              case SortMethod.SORT_DATE:
                _MapSettings.SortBy=(int)SortMethod.SORT_SIZE;
                break;
              case SortMethod.SORT_SIZE:
                _MapSettings.SortBy=(int)SortMethod.SORT_NAME;
                break;
            }
            OnSort();
            GUIControl.FocusControl(GetID,iControl);
          }

          if (iControl==(int)Controls.CONTROL_VIEW)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            int iItem=(int)msg.Param1;
            int iAction=(int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SHOW_INFO) 
            {
              if (m_directory.IsRemote(m_strDirectory)) return true;
              OnInfo(iItem);
            }
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
            if (iAction == (int)Action.ActionType.ACTION_QUEUE_ITEM)
            {
              if (m_directory.IsRemote(m_strDirectory)) return true;
              OnQueueItem(iItem);
            }

          }
          else if (iControl==(int)Controls.CONTROL_BTNSLIDESHOW) // Slide Show
          {
            OnSlideShow();
          }
          else if (iControl==(int)Controls.CONTROL_BTNSLIDESHOW_RECURSIVE) // Recursive Slide Show
          {
            OnSlideShowRecursive();
          }
          else if (iControl==(int)Controls.CONTROL_BTNCREATETHUMBS) // Create Thumbs
          {
            if (m_directory.IsRemote(m_strDirectory)) return true;
            OnCreateThumbs();
          }
          else if (iControl==(int)Controls.CONTROL_BTNROTATE) // Rotate Pic
          {
            OnRotatePicture();
            return true;
          }
          break;

				case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
					m_strDirectory=message.Label;
					OnSlideShowRecursive();
					break;

				case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
					m_strDirectory=message.Label;
					LoadDirectory(m_strDirectory);
					break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
          pControl.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          GUIFacadeControl pControl2=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
          pControl2.OnMessage(message);
          break;

				case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
				case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
					if (m_strDirectory == "" || m_strDirectory.Substring(0,2)==message.Label)
					{
						m_strDirectory = "";
						LoadDirectory(m_strDirectory);
					}
					break;

      }
      return base.OnMessage(message);
    }


    bool ViewByIcon
    {
      get 
      {
        if (_MapSettings.ViewAs != (int)View.VIEW_AS_LIST) return true;
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (_MapSettings.ViewAs == (int)View.VIEW_AS_LARGEICONS) return true;
        return false;
      }
    }

    GUIListItem GetSelectedItem()
    {
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,(int)Controls.CONTROL_VIEW);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      GUIListItem item = GUIControl.GetListItem(GetID,(int)Controls.CONTROL_VIEW,iItem);
      return item;
    }

    int GetSelectedItemNo()
    {

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,(int)Controls.CONTROL_VIEW,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }

    int GetItemCount()
    {
      return GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_VIEW);
    }
    void UpdateButtons()
    {
      string strLine="";
      View view=(View)_MapSettings.ViewAs;
      SortMethod method=(SortMethod )_MapSettings.SortBy;
      bool bAsc=_MapSettings.SortAscending;
      GUIControl.HideControl(GetID, (int)Controls.CONTROL_BTNROTATE);
      switch (view)
      {
        case View.VIEW_AS_LIST:
          strLine=GUILocalizeStrings.Get(101);
          break;
        case View.VIEW_AS_ICONS:
          strLine=GUILocalizeStrings.Get(100);
          break;
        case View.VIEW_AS_LARGEICONS:
          strLine=GUILocalizeStrings.Get(417);
          break;
        case View.VIEW_AS_FILMSTRIP:
          strLine=GUILocalizeStrings.Get(733);
          
          GUIControl.ShowControl(GetID, (int)Controls.CONTROL_BTNROTATE);
          GUIControl.DisableControl(GetID, (int)Controls.CONTROL_BTNROTATE);
          GUIListItem item=GetSelectedItem();
          if (item!=null)
          {
            if (!item.IsFolder)
            {
              GUIControl.EnableControl(GetID, (int)Controls.CONTROL_BTNROTATE);
            }
          }
        break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNVIEWASICONS,strLine);

      switch (method)
      {
        case SortMethod.SORT_NAME:
          strLine=GUILocalizeStrings.Get(103);
          break;
        case SortMethod.SORT_DATE:
          strLine=GUILocalizeStrings.Get(104);
          break;
        case SortMethod.SORT_SIZE:
          strLine=GUILocalizeStrings.Get(105);
          break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY,strLine);

      if (bAsc)
        GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);


    }

    void ShowThumbPanel()
    {
      int iItem=GetSelectedItemNo(); 
      GUIFacadeControl pControl=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
      if ( _MapSettings.ViewAs== (int)View.VIEW_AS_LARGEICONS )
      {
        pControl.View=GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_ICONS)
      {
        pControl.View=GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_LIST)
      {
        pControl.View=GUIFacadeControl.ViewMode.List;
      }
      else if (_MapSettings.ViewAs== (int)View.VIEW_AS_FILMSTRIP)
      {
        pControl.View=GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (iItem>-1)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIEW,iItem);
      }
      UpdateButtons();
    }

		void LoadFolderSettings(string strDirectory)
		{
			if (strDirectory=="") strDirectory="root";
			object o;
			FolderSettings.GetFolderSetting(strDirectory,"Pictures",typeof(GUIPictures.MapSettings), out o);
			if (o!=null) _MapSettings = o as MapSettings;
			if (_MapSettings==null) _MapSettings  = new MapSettings();				
		}
    void SaveFolderSettings(string strDirectory)
    {
      if (strDirectory=="") strDirectory="root";
      FolderSettings.AddFolderSetting(strDirectory,"Pictures",typeof(GUIPictures.MapSettings), _MapSettings);
    }

    void OnRetrieveCoverArt(GUIListItem item)
    {
			if (item.IsRemote) return;
      Utils.SetDefaultIcons(item);
      Utils.SetThumbnails(ref item);
      if (!item.IsFolder)
      {
        string strThumb=GetThumbnail(item.Path) ;
        item.ThumbnailImage=strThumb;
      }
      else
      {
        if (item.Label!="..")
        {
          string strThumb=item.Path+@"\folder.jpg" ;
					if (System.IO.File.Exists(strThumb))
					{
						item.ThumbnailImage=strThumb;
					}
        }
      }
    }

    void LoadDirectory(string strNewDirectory)
    {
      GUIListItem SelectedItem = GetSelectedItem();
      if (SelectedItem!=null) 
      {
        if (SelectedItem.IsFolder && SelectedItem.Label!="..")
        {
          m_history.Set(SelectedItem.Label, m_strDirectory);
        }
      }
      if (strNewDirectory != m_strDirectory && _MapSettings!=null) 
      {
        SaveFolderSettings(m_strDirectory);
      }

      if (strNewDirectory != m_strDirectory || _MapSettings==null) 
      {
        LoadFolderSettings(strNewDirectory);
      }

      m_strDirectory=strNewDirectory;
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_VIEW);
     
      CreateThumbnails();       
      string strObjects="";

      ArrayList itemlist=m_directory.GetDirectory(m_strDirectory);
      Filter(ref itemlist);
      
      string strSelectedItem=m_history.Get(m_strDirectory);	
      int iItem=0;
      foreach (GUIListItem item in itemlist)
      {
        item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_VIEW,item);

      }
      OnSort();
      for (int i=0; i< GetItemCount();++i)
      {
        GUIListItem item =GetItem(i);
        if (item.Label==strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_VIEW,iItem);
          break;
        }
        iItem++;
      }
      int iTotalItems=itemlist.Count;
      if (itemlist.Count>0)
      {
        GUIListItem rootItem=(GUIListItem)itemlist[0];
        if (rootItem.Label=="..") iTotalItems--;
      }
      strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);

      ShowThumbPanel();
    }
    #endregion

    #region Sort Members
    void OnSort()
    {
      GUIFacadeControl list=(GUIFacadeControl)GetControl((int)Controls.CONTROL_VIEW);
      list.Sort(this);
      UpdateButtons();
    }

    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;
      if (item1.IsFolder && item1.Label=="..") return -1;
      if (item2.IsFolder && item2.Label=="..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1; 

      string strSize1="";
      string strSize2="";
      if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);

      SortMethod method=(SortMethod )_MapSettings.SortBy;
      bool bAsc=_MapSettings.SortAscending;

      switch (method)
      {
        case SortMethod.SORT_NAME:
          item1.Label2=strSize1;
          item2.Label2=strSize2;

          if (bAsc)
          {
            return String.Compare(item1.Label ,item2.Label,true);
          }
          else
          {
            return String.Compare(item2.Label ,item1.Label,true);
          }
        

        case SortMethod.SORT_DATE:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          
          item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          if (bAsc)
          {
            return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
          }

        case SortMethod.SORT_SIZE:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          item1.Label2=strSize1;
          item2.Label2=strSize2;
          if (bAsc)
          {
            return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
          }
          else
          {
            return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
          }
      } 
      return 0;
    }
    #endregion

    
    void OnClick(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      if (item.IsFolder)
      {
        m_iItemSelected=-1;
        LoadDirectory(item.Path);
      }
      else
      {
        if (m_directory.IsRemote(item.Path) )
        {
          if (!m_directory.IsRemoteFileDownloaded(item.Path,item.FileInfo.Length) )
          {
            if (!m_directory.ShouldWeDownloadFile(item.Path)) return;
            if (!m_directory.DownloadRemoteFile(item.Path,item.FileInfo.Length))
            {
              //show message that we are unable to download the file
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,0,0,0,0,0,0);
              msg.Param1=916;
              msg.Param2=920;
              msg.Param3=0;
              msg.Param4=0;
              GUIWindowManager.SendMessage(msg);

              return;
            }
          }
          return;
        }

        m_iItemSelected=GetSelectedItemNo();
        OnShowPicture(item.Path);  
      }
    }
    
    void OnQueueItem(int iItem)
    {
    }

    void OnShowPicture(string strFile)
    {
      GUISlideShow SlideShow = (GUISlideShow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow==null) return;
      

      SlideShow.Reset();
      for (int i=0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (!item.IsFolder)
        {
          if (item.IsRemote) continue;
          SlideShow.Add(item.Path);
        }
      }
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      SlideShow.Select(strFile);
    }

    void AddDir(GUISlideShow SlideShow ,string strDir)
    {
      ArrayList itemlist=m_directory.GetDirectory(strDir);
      Filter(ref itemlist);
      foreach (GUIListItem item in itemlist)
      {
        if (item.IsFolder)
        {
          if (item.Label!="..")
            AddDir(SlideShow,item.Path);
        }
        else if (!item.IsRemote)
        {
          SlideShow.Add(item.Path);
        }
      }
    }

    void OnSlideShowRecursive()
    {
      GUISlideShow SlideShow = (GUISlideShow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow==null) return;
      
      SlideShow.Reset();
      AddDir(SlideShow, m_strDirectory);
      SlideShow.StartSlideShow();
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
    }

    void OnSlideShow()
    {
      OnSlideShow(0);
    }
    void OnSlideShow(int iStartItem)
    {
      GUISlideShow SlideShow = (GUISlideShow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow==null) return;     

      SlideShow.Reset();

      if ((iStartItem<0) || (iStartItem>GetItemCount())) iStartItem=0;
      int i=iStartItem;
      do
      {
        GUIListItem item = GetItem(i);
        if (!item.IsFolder && !item.IsRemote)
        {
          SlideShow.Add(item.Path);
        }

        i++;
        if (i >= GetItemCount())
        {
          i=0;
        }
      }
      while (i != iStartItem);

      SlideShow.StartSlideShow();
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
    }

    public bool ThumbnailCallback()
    {
      return false;
    }

    void OnCreateThumbs()
    {
      CreateFolderThumbs();
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress!=null)
      {
        dlgProgress.SetHeading(110);
        dlgProgress.SetLine(1,"");
        dlgProgress.SetLine(2,"");
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }


      using (PictureDatabase dbs = new PictureDatabase())
      {
        for (int i=0; i < GetItemCount(); ++i)
        {
          GUIListItem item = GetItem(i);
          if (item.IsRemote) continue;
          if (!item.IsFolder)
          {
            if (Utils.IsPicture(item.Path))
            {
              string strProgress=String.Format("progress:{0}/{1}", i+1, GetItemCount() );
              string strFile=String.Format("picture:{0}", item.Label);
              if (dlgProgress!=null)
              {
                dlgProgress.SetLine(1, strFile);
                dlgProgress.SetLine(2, strProgress);
                dlgProgress.Progress();
                if ( dlgProgress.IsCanceled ) break;
              }


              string strThumb=GetThumbnail(item.Path );
              int iRotate=dbs.GetRotation(item.Path);
              Util.Picture.CreateThumbnail(item.Path,strThumb,128,128,iRotate);
            }
          }
        }
      }
      if (dlgProgress!=null) dlgProgress.Close();
      GUITextureManager.CleanupThumbs();
      LoadDirectory(m_strDirectory);
    }

    void CreateFolderThumbs()
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress!=null)
      {
        dlgProgress.SetHeading(110);
        dlgProgress.SetLine(1,"");
        dlgProgress.SetLine(2,"");
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      for (int i=0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label!="..")
        {
          string strProgress=String.Format("progress:{0}/{1}", i+1, GetItemCount() );
          string strFile=String.Format("folder:{0}", item.Label);
          if (dlgProgress!=null)
          {
            dlgProgress.SetLine(1, strFile);
            dlgProgress.SetLine(2, strProgress);
            dlgProgress.Progress();
            if ( dlgProgress.IsCanceled ) break;
          }

          CreateFolderThumb(item.Path);
        }//if (item.IsFolder)
      }//for (int i=0; i < GetItemCount(); ++i)
      if (dlgProgress!=null) dlgProgress.Close();
    }

    void CreateFolderThumb(string path)
    {
      // find first 4 jpegs in this subfolder
      ArrayList itemlist=m_directory.GetDirectoryUnProtected(path,true);
      Filter(ref itemlist);
      ArrayList m_pics=new ArrayList();
      foreach (GUIListItem subitem in itemlist)
      {
        if (!subitem.IsFolder)
        {
          if ( Utils.IsPicture(subitem.Path) )
          {
            m_pics.Add(subitem.Path);
            if (m_pics.Count>=4) break;
          }
        }
      }
      if (m_pics.Count>0)
      {
        using (Image imgFolder=Image.FromFile(GUIGraphicsContext.Skin+@"\media\previewbackground.png") )
        {
          int iWidth=imgFolder.Width;
          int iHeight=imgFolder.Height;
          
          int iThumbWidth=(iWidth-30)/2;
          int iThumbHeight=(iHeight-30)/2;

          using (Bitmap bmp = new Bitmap(iWidth,iHeight))
          {
            using (Graphics g = Graphics.FromImage(bmp) )
            {
              g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
              g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
              g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
  
              g.DrawImage(imgFolder,0,0,iWidth,iHeight);

              int x,y,w,h;
              x=0;y=0;w=iThumbWidth;h=iThumbHeight;
              using (Image img=LoadPicture((string)m_pics[0]))
              {
                g.DrawImage(img,x+10,y+10,w,h);
              }
              
              if (m_pics.Count>1)
              {
                using (Image img=LoadPicture((string)m_pics[1]))
                {
                  g.DrawImage(img,x+iThumbWidth+20,y+10,w,h);
                }
              }
              
              if (m_pics.Count>2)
              {
                using (Image img=LoadPicture((string)m_pics[2]))
                {
                  g.DrawImage(img,x+10,y+iThumbHeight+20,w,h);
                }
              }
              if (m_pics.Count>3)
              {
                using (Image img=LoadPicture((string)m_pics[3]))
                {
                  g.DrawImage(img,x+iThumbWidth+20,y+iThumbHeight+20,w,h);
                }
              }
            }//using (Graphics g = Graphics.FromImage(bmp) )
            try
            {
              string strThumbName=path+@"\folder.jpg";
              Utils.FileDelete(strThumbName);
              bmp.Save(strThumbName,System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception)
            {
            }
          }//using (Bitmap bmp = new Bitmap(210,210))
        }
      }//if (m_pics.Count>0)
    }

    Image LoadPicture(string strFileName)
    {
      Image img = null;
      using (PictureDatabase dbs= new PictureDatabase())
      {
        int iRotate=dbs.GetRotation(strFileName);
        img = Image.FromFile(strFileName);
        if (img!=null)
        {
          if (iRotate>0)
          {
            RotateFlipType fliptype;
            switch (iRotate)
            {
              case 1:
                fliptype=RotateFlipType.Rotate90FlipNone;
                img.RotateFlip(fliptype);
                break;
              case 2:
                fliptype=RotateFlipType.Rotate180FlipNone;
                img.RotateFlip(fliptype);
                break;
              case 3:
                fliptype=RotateFlipType.Rotate270FlipNone;
                img.RotateFlip(fliptype);
                break;
              default:
                fliptype=RotateFlipType.RotateNoneFlipNone;
                break;
            }
          }
        }
      }
      return img;
    }

    void Filter(ref ArrayList itemlist)
    {
      bool bFound;
      do
      {
        bFound=false;
        for (int i=0; i < itemlist.Count;++i)
        {
          GUIListItem item=(GUIListItem) itemlist[i];
          if (!item.IsFolder)
          {
            if ( item.Path.IndexOf("folder.jpg") > 0 )
            {
              bFound=true;
              itemlist.RemoveAt(i);
              break;
            }
          }
        }
      } while (bFound);
		}

    static public string GetThumbnail(string strPhoto)
    {
      if (strPhoto==String.Empty) return String.Empty;		
      return String.Format(@"{0}\{1}.jpg",ThumbsFolder,Utils.EncryptLine(strPhoto) );
    }
    static public string GetLargeThumbnail(string strPhoto)
    {
      if (strPhoto==String.Empty) return String.Empty;
      return String.Format(@"{0}\{1}L.jpg",ThumbsFolder,Utils.EncryptLine(strPhoto) );
    }

		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

    public bool HasSetup()
    {
      return false;
    }
		public string PluginName()
		{
			return "My Pictures";
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			// TODO:  Add GUIPictures.GetHome implementation
			strButtonText = GUILocalizeStrings.Get(1);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string Description()
		{
			return "Plugin to watch your photo's";
		}

		public void ShowPlugin()
		{
			// TODO:  Add GUIPictures.ShowPlugin implementation
		}

    #endregion

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip=parent as GUIFilmstripControl ;
      if (filmstrip==null) return;
      string strThumb=GetLargeThumbnail(item.Path );
      filmstrip.InfoImageFileName=strThumb;
      UpdateButtons();
    }

    void OnRotatePicture()
    {
      GUIListItem item=GetSelectedItem();
      if (item==null) return;
      if (item.IsFolder) return;
      if (item.IsRemote) return;
      int rotate=0;
      using (PictureDatabase dbs = new PictureDatabase())
      {
        rotate=dbs.GetRotation(item.Path);
        rotate++;
        if (rotate>=4)
        {
          rotate=0;
        }
        dbs.SetRotation(item.Path,rotate);
      }
      string strThumb=GetThumbnail(item.Path );
      Util.Picture.CreateThumbnail(item.Path,strThumb,128,128,rotate);

      strThumb=GetLargeThumbnail(item.Path) ;
      Util.Picture.CreateThumbnail(item.Path,strThumb,512,512,rotate);
      System.Threading.Thread.Sleep(100);
      GUIControl.RefreshControl(GetID, (int)Controls.CONTROL_VIEW);      
    }

    void CreateThumbnails()
    {
      Thread WorkerThread = new Thread(new ThreadStart(WorkerThreadFunction));

      WorkerThread.Start();
    }
    
    void WorkerThreadFunction()
    {
      string path=m_strDirectory;
      ArrayList itemlist=m_directory.GetDirectoryUnProtected(path,true);
      using (PictureDatabase dbs = new PictureDatabase())
      {
        foreach (GUIListItem item in itemlist)
        {
          if (m_strDirectory!=path) return;
          if (GUIWindowManager.ActiveWindow!=GetID) return;
          if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;
          if (!item.IsFolder)
          {
            if (Utils.IsPicture(item.Path) )
            {
              string strThumb=GetThumbnail(item.Path) ;
              if (!System.IO.File.Exists(strThumb))
              {
                int iRotate=dbs.GetRotation(item.Path);
                Util.Picture.CreateThumbnail(item.Path,strThumb,128,128,iRotate);
                System.Threading.Thread.Sleep(100);
              }

              strThumb=GetLargeThumbnail(item.Path) ;
              if (!System.IO.File.Exists(strThumb))
              {
                int iRotate=dbs.GetRotation(item.Path);
                Util.Picture.CreateThumbnail(item.Path,strThumb,512,512,iRotate);
                System.Threading.Thread.Sleep(100);
              }
            }
          }
          else
          {
            if (item.Label!="..")
            {
              string strThumb=item.Path+@"\folder.jpg" ;
              if (!System.IO.File.Exists(strThumb))
              {
                CreateFolderThumb(item.Path);
                System.Threading.Thread.Sleep(100);
              }
            }
          }      
        } //foreach (GUIListItem item in itemlist)
      } //using (PictureDatabase dbs = new PictureDatabase())
    } //void WorkerThreadFunction()

		void ShowContextMenu()
		{
			GUIListItem item=GetSelectedItem();
			m_itemItemSelected=item;
			int itemNo=GetSelectedItemNo();
			m_iItemSelected=itemNo;

			if (item==null) return;
      if (item.IsFolder && item.Label=="..") return;

      GUIControl cntl=GetControl((int)Controls.CONTROL_VIEW);
      if (cntl==null) return; // Control not found

			GUIDialogMenu	dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu
			if (!item.IsFolder)
			{
				dlg.AddLocalizedString(735); //rotate
				dlg.AddLocalizedString(923); //show
				dlg.AddLocalizedString(108); //start slideshow
				dlg.AddLocalizedString(940); //properties
			}
			if (!item.IsRemote && m_bFileMenuEnabled)
			{
				dlg.AddLocalizedString(500); // FileMenu
			}

			dlg.DoModal(GetID);
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 117: // delete
					OnDeleteItem(item);
					break;

				case 735: // rotate
					OnRotatePicture();
				break;

				case 923: // show
					OnClick(itemNo);	
					break;

        case 108: // start slideshow
          OnSlideShow(itemNo);	
          break;

				case 940: // properties
					OnInfo(itemNo);	
					break;

				case 500: // File menu
				{
					// get pincode
					if (m_strFileMenuPinCode != "")
					{
						string strUserCode="";
						if (GetUserInputString(ref strUserCode) && strUserCode==m_strFileMenuPinCode)
						{
							ShowFileMenu();
						}
					}
					else 
						ShowFileMenu();
				}
					break;
			}
		}
		
		void ShowFileMenu()
		{
			bool bReload = false;

			GUIListItem item=m_itemItemSelected;
			if (item==null) return;
			if (item.IsFolder && item.Label=="..") return;

			GUIControl cntl=GetControl((int)Controls.CONTROL_VIEW);
			if (cntl==null) return; // Control not found

			GUIDialogMenu	dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(500); // File menu
	
			if (m_strDestination != "")
			{
				dlg.AddLocalizedString(115); //copy
				if (!Utils.IsDVD(item.Path)) dlg.AddLocalizedString(116); //move					
			}
			if (!Utils.IsDVD(item.Path)) dlg.AddLocalizedString(118); //rename				
			if (!Utils.IsDVD(item.Path)) dlg.AddLocalizedString(117); //delete
			if (!Utils.IsDVD(item.Path)) dlg.AddLocalizedString(119); //new folder

			if (item.IsFolder && !Utils.IsDVD(item.Path))
			{
				dlg.AddLocalizedString(501); // Set as destination
			}
			if (m_strDestination != "") dlg.AddLocalizedString(504); // Goto destination
			     
			dlg.DoModal(GetID);
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 117: // delete
					OnDeleteItem(item);
					break;

				case 118: // rename
				{
					string strSourceName = "";
					if (item.IsFolder)
						strSourceName = System.IO.Path.GetFileName(item.Path);
					else
						strSourceName = System.IO.Path.GetFileNameWithoutExtension(item.Path);

					string strExtension = System.IO.Path.GetExtension(item.Path);
					string strDestinationName = strSourceName;
					if (GetUserInputString(ref strDestinationName) == true)
					{
						if (item.IsFolder)
						{
							// directory rename
							if (Directory.Exists(m_strDirectory+"\\"+strSourceName))
							{
								try
								{
									Directory.Move(m_strDirectory+"\\"+strSourceName, m_strDirectory+"\\"+strDestinationName);
								}
								catch(Exception) 
								{
									ShowError(dlg.SelectedId, m_strDirectory+"\\"+strSourceName);
								}
								bReload = true;
							}
						}
						else
						{
							// file rename
							if (File.Exists(item.Path))
							{
								string strDestinationFile = m_strDirectory+"\\"+strDestinationName+strExtension;
								try
								{									
									File.Move(item.Path, strDestinationFile);
								}
								catch(Exception) 
								{
									ShowError(dlg.SelectedId, m_strDirectory+"\\"+strSourceName);
								}
								bReload = true;
							}
						}						
					}
				}
					break;

				case 115: // copy				
					{
						FileItemMove(false, item);
					}
					break;

				case 116: // move
					{
						FileItemMove(true, item);
						bReload = true;
					}
					break;
				
				case 119: // make dir
				{
					MakeDir();
					bReload = true;
				}
					break;

				case 501: // set as destiantion
					m_strDestination = System.IO.Path.GetFullPath(item.Path)+"\\";					
					break;

				case 504: // goto destination
				{
					m_strDirectory = m_strDestination;
					m_iItemSelected = -1;
					bReload = true;
				}
					break;
			}

			if (bReload)
			{
				LoadDirectory(m_strDirectory);
				if (m_iItemSelected>=0)
				{
					GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_VIEW,m_iItemSelected);
				}
			}
		}

		bool GetUserInputString(ref string sString)
		{			
			VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);			
			keyBoard.Reset();
			keyBoard.Text = sString;
			keyBoard.DoModal(GetID); // show it...
			System.GC.Collect(); // collect some garbage
			if (keyBoard.IsConfirmed) sString=keyBoard.Text;
			return keyBoard.IsConfirmed;
		}

		void ShowError(int iAction, string SourceOfError)
		{
			GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			if(dlgOK !=null)
			{
				dlgOK.SetHeading(iAction);
				dlgOK.SetLine(1,SourceOfError);
				dlgOK.SetLine(2,502);
				dlgOK.DoModal(GetID);
			}
		}

		void MakeDir() 
		{
			// Get input string
			string verStr = "";
			GetUserInputString(ref verStr);

			// Ask user confirmation
				string path = m_strDirectory+"\\"+verStr;
			try 
			{
				// Determine whether the directory exists.
				if (Directory.Exists(path)) 
				{
					GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
					dlgOk.SetHeading(119); 
					dlgOk.SetLine(1, 2224);
					dlgOk.SetLine(2, "");
					dlgOk.DoModal(GetID);
				} 
				else 
				{
					DirectoryInfo di = Directory.CreateDirectory(path);
				}
			}
			catch (Exception )
			{
				ShowError(119, path);
			}
		}

		/// <summary>
		/// Moves or Copy a file
		/// </summary>
		/// <param name="bMove">Move or Copy (Move=true)</param>
		/// <param name="item">Item to be move or copied</param>
		void FileItemMove(bool bMove, GUIListItem item) 
		{
			// init
			GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILE);
			if (dlgFile == null) return;
			
			// File operation settings
			if (bMove) dlgFile.SetMode(1); // move
			else dlgFile.SetMode(0); // copy
			dlgFile.SetSourceItem(item);
			dlgFile.SetDestinationDir(m_strDestination);
			
			// move
			dlgFile.DoModal(GetID);

			//final
			if (null!=dlgFile) dlgFile.Close();
		}
		
		void OnDeleteItem(GUIListItem item)
		{
			if (item.IsRemote) return;

			GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (null==dlgYesNo) return;
			string strFileName=System.IO.Path.GetFileName(item.Path);
			if (!item.IsFolder) dlgYesNo.SetHeading(664);
			else dlgYesNo.SetHeading(503);
			dlgYesNo.SetLine(1,strFileName);
			dlgYesNo.SetLine(2, "");
			dlgYesNo.SetLine(3, "");
			dlgYesNo.DoModal(GetID);

			if (!dlgYesNo.IsConfirmed) return;
			DoDeleteItem(item);
						
			m_iItemSelected=GetSelectedItemNo();
			if (m_iItemSelected>0) m_iItemSelected--;
			LoadDirectory(m_strDirectory);
			if (m_iItemSelected>=0)
			{
				GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_VIEW,m_iItemSelected);
			}
		}

		void DoDeleteItem(GUIListItem item)
		{
			if (item.IsFolder)
			{
        if (item.Label != "..")
        {
          ArrayList items = new ArrayList();
          items=m_directory.GetDirectoryUnProtected(item.Path,false);
          foreach(GUIListItem subItem in items)
          {
            DoDeleteItem(subItem);
          }
          Utils.DirectoryDelete(item.Path);
        }
			}
			else if (!item.IsRemote)
			{  			
        Utils.FileDelete(item.Path);
			}
		}

		void OnInfo(int itemNumber)
		{
			GUIListItem item=GetItem(itemNumber);
			if (item==null) return;
			if (item.IsFolder || item.IsRemote) return;
			GUIDialogExif exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_EXIF);
			exifDialog.FileName=item.Path;
			exifDialog.DoModal(GetID);
		}
  }
}
