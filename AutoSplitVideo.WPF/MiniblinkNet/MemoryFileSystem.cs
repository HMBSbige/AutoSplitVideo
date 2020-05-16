using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Kyozy.MiniblinkNet
{
    /// <summary>
    /// 内存文件系统，订阅 OnDataLoaded 事件，并在事件参数中设置内存数据
    /// </summary>
    public class MemoryFileSystem : FileSystem
    {
        //private Dictionary<GCHandle, MemoryStream> m_strems;
        /// <summary>
        /// 数据载入事件
        /// </summary>
        public event EventHandler<DataLoadedEventArgs> OnDataLoaded;

        public override IntPtr FileOpen(IntPtr Path)
        {
            string fileName = Help.PtrToStringUTF8(Path);
            if (OnDataLoaded != null)
            {
                DataLoadedEventArgs e = new DataLoadedEventArgs(fileName);
                OnDataLoaded(this, e);
                if (e.Data == null)
                {
                    e.Data = File.ReadAllBytes(fileName);
                    if (e.Data == null)
                    {
                        return IntPtr.Zero;
                    }
                }
                MemoryStream ms = new MemoryStream(e.Data);
                GCHandle handle = GCHandle.Alloc(ms);
                return GCHandle.ToIntPtr(handle);

            }
            return IntPtr.Zero;
        }

        public override void FileClose(IntPtr handle)
        {
            try
            {
                if (handle != IntPtr.Zero)
                {
                    GCHandle gch = GCHandle.FromIntPtr(handle);
                    MemoryStream ms = (MemoryStream)gch.Target;
                    ms.Dispose();
                    gch.Free();
                }
            }
            catch (InvalidOperationException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

        }

        public override int FileRead(IntPtr handle, IntPtr buffer, int size)
        {
            try
            {
                if (handle != IntPtr.Zero)
                {
                    GCHandle gch = GCHandle.FromIntPtr(handle);
                    MemoryStream ms = (MemoryStream)gch.Target;
                    byte[] buff = new byte[size];
                    int len = ms.Read(buff, 0, size);
                    Marshal.Copy(buff, 0, buffer, size);
                    return len;
                }
            }
            catch (InvalidOperationException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
            return 0;
        }

        public override int FileSeek(IntPtr handle, int offset, int origin)
        {
            try
            {
                if (handle != IntPtr.Zero)
                {
                    GCHandle gch = GCHandle.FromIntPtr(handle);
                    MemoryStream ms = (MemoryStream)gch.Target;
                    return (int)ms.Seek(offset, (SeekOrigin)origin);
                }
            }
            catch (InvalidOperationException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
            return 0;
        }

        public override int FileSize(IntPtr handle)
        {
            try
            {
                if (handle != IntPtr.Zero)
                {
                    GCHandle gch = GCHandle.FromIntPtr(handle);
                    MemoryStream ms = (MemoryStream)gch.Target;
                    return (int)ms.Length;
                }
            }
            catch (InvalidOperationException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
            return 0;
        }
    }

    /// <summary>
    /// 数据载入事件参数，如果没有设置数据则从文件读
    /// </summary>
    public class DataLoadedEventArgs : EventArgs
    {
        private string m_path;
        private byte[] m_data;

        public DataLoadedEventArgs(string path)
        {
            m_path = path;
        }
        /// <summary>
        /// 设置 byte[] 数据
        /// </summary>
        public byte[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }
        /// <summary>
        /// 设置图片数据
        /// </summary>
        public System.Drawing.Image ImageData
        {
            get 
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(m_data))
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        return img;
                    }
                    
                }
                catch(ArgumentNullException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return null;
                }
            }
            set
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    m_data = ms.GetBuffer();
                }
            }
        }
        /// <summary>
        /// 设置字符串UTF8数据
        /// </summary>
        public string StringData
        {
            get 
            {
                return Encoding.UTF8.GetString(m_data);
            }
            set 
            {
                m_data = Encoding.UTF8.GetBytes(value);
            }
        }
        /// <summary>
        /// 获取文件路径
        /// </summary>
        public string Path
        {
            get { return m_path; }
        }

    }
}
