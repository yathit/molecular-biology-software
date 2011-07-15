using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows;
using System.IO;
using System.Windows.Media;

namespace BaseLibrary
{
    public class UserSetting : ApplicationSettingsBase
    {
        #region constructors and static variables
        public const string APPNAME = "MolBio";
        static readonly private string DEFAULTDOCFOLDER;

        static public UserSetting INSTANT;

        static UserSetting()
        {
            DEFAULTDOCFOLDER = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), 
                APPNAME);
            INSTANT = new UserSetting();

        }

        #endregion

        #region Miscellaneous setting

        #region Canvas setting

        /// <summary>
        /// Number of character in a line including space in <code>SeqTextBlock</code>
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("60")]
        public int SeqText_LineWidth
        {
            get
            {
                int width = (int)this["SeqText_LineWidth"];
                if (width < 1)
                    width = 60;
                return width;
            }
            set
            {
                this["SeqText_LineWidth"] = value;
            }
        }

        /// <summary>
        /// Line width in SeqTextBlock
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("12")]
        public double SeqText_FontSize
        {
            get
            {
                double size = (double)this["SeqText_FontSize"];
                if (size < 1)
                    size = 12;
                return size;
            }
            set
            {
                this["SeqText_LineWidth"] = value;
            }
        }

        /// <summary>
        /// Word size. A space is placed interval of word size in sequence canvas.
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("10")]
        public int SeqText_WordSize
        {
            get
            {
                int size = (int)this["SeqText_WordSize"];
                if (size < 1)
                    size = 10;
                return size;
            }
            set
            {
                this["SeqText_WordSize"] = value;
            }
        }

        /// <summary>
        /// Shown text as capatalized
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public bool? SeqText_Capatalized
        {
            get
            {
                return (bool?)this["SeqText_Capatalized"];
            }
            set
            {
                this["SeqText_Capatalized"] = value;
            }
        }

        /// <summary>
        /// Font family in seq canvas
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Lucida Console")]
        public string SeqText_FontFamily
        {
            get
            {
                return (string)this["SeqText_FontFamily"];
            }
            set
            {
                this["SeqText_FontFamily"] = value;
            }
        }


        Typeface _seqText_Typeface = null;
        public Typeface SeqText_Typeface
        {
            get
            {
                if (_seqText_Typeface == null)
                    _seqText_Typeface = new Typeface(SeqText_FontFamily);
                return _seqText_Typeface;
            }
        }

        /// <summary>
        /// Font family in GenBank canvas
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Lucida Console")]
        public string GenbankCanvas_FontFamily
        {
            get
            {
                return (string)this["GenbankCanvas_FontFamily"];
            }
            set
            {
                this["GenbankCanvas_FontFamily"] = value;
            }
        }

        /// <summary>
        /// Line width in GenbankCanvas
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("12")]
        public double GenbankCanvas_FontSize
        {
            get
            {
                double size = (double)this["GenbankCanvas_FontSize"];
                if (size < 1)
                    size = 12;
                return size;
            }
            set
            {
                this["GenbankCanvas_FontSize"] = value;
            }
        }

        /// <summary>
        /// Default Layer width
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("16")]
        public double Layer_Circular_DefaultLayerWidth
        {
            get
            {
                double size = (double)this["Layer_Circular_DefaultLayerWidth"];
                if (size < 1) size = 12;
                return size;
            }
            set
            {
                this["Layer_Circular_DefaultLayerWidth"] = value;
            }
        }


        /// <summary>
        /// Default Layer width
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("64")]
        public int Feature_Label_MaxLength
        {
            get
            {
                int size = (int)this["Feature_Label_MaxLength"];
                if (size < 1) size = 64;
                return size;
            }
            set
            {
                this["Feature_Label_MaxLength"] = value;
            }
        }

        /// <summary>
        /// Default Layer width
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("20")]
        public double Layer_Linear_DefaultLayerWidth
        {
            get
            {
                double size = (double)this["Layer_Linear_DefaultLayerWidth"];
                if (size < 1) size = 12;
                return size;
            }
            set
            {
                this["Layer_Linear_DefaultLayerWidth"] = value;
            }
        }

        

        #endregion

        #endregion

        #region File and folder setting
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public string Folder_Document
        {
            get
            {
                string dir = (string)this["Folder_Document"];
                if (dir == null || dir.Length == 0)
                {
                    dir = DEFAULTDOCFOLDER;
                }
                if (!System.IO.Directory.Exists(dir))
                    try
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to create default folder: " + dir, 
                            "Creating user directory", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                return dir;
            }
            set
            {
                if (value != null && System.IO.Directory.Exists(value))
                {
                    this["Folder_Document"] = value;
                }
            }
        }

        /// <summary>
        /// Last open folder
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public string Folder_LastOpen
        {
            get
            {
                string dir = (string)this["Folder_LastOpen"];
                if (dir == null || dir.Length == 0)
                    dir = Folder_Document;
                return dir;
            }
            set
            {
                if (value != null && System.IO.Directory.Exists(value))
                    this["Folder_LastOpen"] = value;
            }
        }


        /// <summary>
        /// Last save folder
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public string Folder_LastSave
        {
            get
            {
                string dir = (string)this["Folder_LastSave"];
                if (dir == null || dir.Length == 0)
                    dir = Folder_Document;
                return dir;
            }
            set
            {
                if (value != null && System.IO.Directory.Exists(value))
                    this["Folder_LastSave"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public string LayoutFileName
        {
            get { return Path.Combine(Folder_UserSetting, "layout.xml"); }
        }

        [UserScopedSetting()]
        public string FileName_Rebase_Enzyme
        {
            get { return Path.Combine(Folder_UserSetting, "rebase_e.txt"); }
        }

        /// <summary>
        /// File name for serialize xml file for fill of a feature
        /// </summary>
        [UserScopedSetting()]
        public string FileName_Annotation
        {
            get { return Path.Combine(Folder_UserSetting, "annot.set"); }
        }

        /// <summary>
        /// File name for serialize xml file for stoke of a feature
        /// </summary>
        [UserScopedSetting()]
        public string FileName_AnnotationStoke
        {
            get { return Path.Combine(Folder_UserSetting, "annot_stoke.xml"); }
        }

        /// <summary>
        /// Folder path for user specific application setting
        /// </summary>
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public string Folder_UserSetting
        {
            get
            {
                string dir = (string)this["Folder_UserSetting"];
                if (dir == null || dir.Length == 0)
                    dir = Folder_Document;
                return dir;
            }
            set
            {
                if (value != null && System.IO.Directory.Exists(value))
                    this["Folder_UserSetting"] = value;
            }
        }


        #endregion

        #region Main window
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("600, 400")]
        public System.Windows.Size MainForm_Size
        {
            get { return (Size) this["MainForm_Size"]; }
            set { this["MainForm_Size"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("50, 50, 800, 600")]
        public string MainForm_Bound
        {
            get { return (string) this["MainForm_Bound"]; }
            set { this["MainForm_Bound"] = value; }
        }
        #endregion

    }
}
