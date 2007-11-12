#region Copyright (C) 2007 Team MediaPortal

/* 
 *	Copyright (C) 2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using MediaPortal.Services;
using MediaPortal.ServiceImplementations;
using MediaPortal.TagReader;
using MediaPortal.Threading;


namespace MediaPortal.Util
{
    /// <summary>
    /// searches for album art and stores it as folder.jpg in the mp3 directory
    /// </summary>
    public class FolderThumbCreator
    {
        string _filename = string.Empty;
        MusicTag _filetag = null;
        Work work;

        // Filename is a full path+file
        public FolderThumbCreator(string Filename, MusicTag FileTag)
        {
            _filename = Filename;
            _filetag = FileTag;
            work = new Work(new DoWorkHandler(this.PerformRequest));
            work.ThreadPriority = ThreadPriority.Lowest;

            GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
        }

        private void PerformRequest()
        {
            lock (this)
            {
                MusicTag musicTag = _filetag;
                string filename = _filename;
                string strFolderThumb = string.Empty;
                strFolderThumb = MediaPortal.Util.Utils.GetLocalFolderThumb(filename);

                string strRemoteFolderThumb = string.Empty;
                //strRemoteFolderThumb = String.Format(@"{0}\folder.jpg", MediaPortal.Util.Utils.RemoveTrailingSlash(filename));
                strRemoteFolderThumb = MediaPortal.Util.Utils.GetFolderThumb(filename);

                if (!System.IO.File.Exists(strRemoteFolderThumb))
                {
                    // no folder.jpg in this share but maybe there's downloaded album art we can save now.
                    try
                    {
                        if (musicTag != null && musicTag.Album != string.Empty && musicTag.Artist != string.Empty)
                        {
                            string albumThumb = Util.Utils.GetAlbumThumbName(musicTag.Artist, musicTag.Album);

                            if (System.IO.File.Exists(albumThumb))
                            {
                                string largeAlbumThumb = Util.Utils.ConvertToLargeCoverArt(albumThumb);
                                if (System.IO.File.Exists(largeAlbumThumb))
                                    System.IO.File.Copy(largeAlbumThumb, strRemoteFolderThumb, false);
                                else
                                    System.IO.File.Copy(albumThumb, strRemoteFolderThumb, false);

                                Log.Info("GUIMusicFiles: Using album art for missing folder thumb {0}", strRemoteFolderThumb);


                                // now we need to cache that new thumb, too
                                if (System.IO.File.Exists(strRemoteFolderThumb))
                                {
                                    try
                                    {
                                        Log.Info("GUIMusicFiles: On-Demand-Creating missing folder thumb cache for {0}", strRemoteFolderThumb);
                                        string localFolderLThumb = Util.Utils.ConvertToLargeCoverArt(strFolderThumb);

                                        if (!System.IO.File.Exists(strFolderThumb))
                                            MediaPortal.Util.Picture.CreateThumbnail(strRemoteFolderThumb, strFolderThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
                                        if (!System.IO.File.Exists(localFolderLThumb))
                                        {
                                            // just copy the folder.jpg if it is reasonable in size - otherwise re-create it
                                            System.IO.FileInfo fiRemoteFolderArt = new System.IO.FileInfo(strRemoteFolderThumb);
                                            if (fiRemoteFolderArt.Length < 32000)
                                                System.IO.File.Copy(strRemoteFolderThumb, localFolderLThumb, true);
                                            else
                                                MediaPortal.Util.Picture.CreateThumbnail(strRemoteFolderThumb, localFolderLThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
                                        }
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
        }
    }
}