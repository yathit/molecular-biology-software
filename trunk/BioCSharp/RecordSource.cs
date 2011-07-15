using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Win32;
using System.Security.Permissions;
using BioCSharp;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BioCSharp.Seqs;

namespace BioCSharp
{
    namespace Data
    {
        public class SourceRecordCollection : ObservableCollection<SourceRecord>
        {
            protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
            {
                base.OnPropertyChanged(e);
                //Window1.log(this, "Change for " + e.PropertyName);
            }
        }

        /// <summary>
        /// Flexible record source for explorer tree view stored in <code>TABLE_SEQRECORD</code> database
        /// table.
        /// </summary>
        public class SourceRecord
        {
            private readonly RecordDataSource source;
            /// <summary>
            /// Master table id
            /// 
            /// </summary>
            private readonly long id = -1;
            public long Id { get { return id; } }
            private SourceRecordCollection children;

            public SourceRecord(RecordDataSource database_source, long master_table_id)
            {
                this.id = master_table_id;
                source = database_source;
                source.ListItems.Add(this);
            }

            public string Name
            {
                get
                {
                    DataRow row = source.DataSet.Tables[RecordDataSource.TABLE_MASTER].Rows.Find(id);
                    if (row != null)
                        return row.Field<string>(RecordDataSource.COL_NAME);
                    else
                        return "";
                }

                set
                {
                    //MessageBox.Show("Will change later to: " + value);
                }
            }

            public SourceRecordCollection Children
            {
                get
                {
                    if (children != null)
                    {
                        //Window1.log(this, "Found " + children.Count + " children from " + Name + " (catch)");
                        return children;
                    }
                    children = new SourceRecordCollection();
                    try
                    {
                        DataRow row = source.DataSet.Tables[RecordDataSource.TABLE_MASTER].Rows.Find(id);
                        if (row.Field<DataTypes>(RecordDataSource.COL_TYPE) == DataTypes.Folder)
                        {
                            foreach (DataRow child_row in row.GetChildRows(RecordDataSource.REL_FOLDER))
                            {
                                children.Add(new SourceRecord(source, child_row.Field<long>(RecordDataSource.COL_ID)));
                            }
                        }
                    }
                    catch
                    {
                    }
                    //Window1.log(this, "Found " + children.Count + " children from " + Name);

                    return children;
                }
                set
                {
                    //Window1.log(this, "Setting ");
                }
            }
        }

        /// <summary>
        /// Data types in <code>RecordDataSource.TABLE_MASTER</code>
        /// 
        /// For future version compatibility, use int assignment. 
        /// </summary>
        [Flags]
        enum DataTypes { Unknown = 0, File = 1, Folder = 2, Sequence = 4, Insdc = 8, Note = 16 };

        public class RecordDataSource
        {

            /// <summary>
            /// Database table representing folder orginization
            /// </summary>
            public const string TABLE_REL_FOLDER = "folder";
            /// <summary>
            /// Relationship database table for folder to sequence record 
            /// </summary>
            // public const string TABLE_REL_SEQRECORD = "folder2seqrecord";
            /// <summary>
            /// Masterdatabase table 
            /// </summary>
            public const string TABLE_MASTER = "master";
            public const string TABLE_SEQUENCE = "sequence";

            public const string TABLE_ACCESSION = "accesion";

            /// <summary>
            /// Column name in <code>TABLE_SEQRECORD</code> and <code>TABLE_MASTER</code>
            /// </summary>
            public const string COL_NAME = "name";
            /// <summary>
            /// Column name in <code>TABLE_MASTER</code> representing type of data as 
            /// specify in <code>DataTypes</code>.
            /// </summary>
            public const string COL_TYPE = "type";
            /// <summary>
            /// (typeof(long)) Column name in <code>TABLE_MASTER</code> serving as primary key. This key is extensively 
            /// used in relationship.
            /// 
            /// Other uses:
            /// Simple relationship table has only two columns: <code>COL_ID</code> or <code>COL_KEY</code> 
            /// (for string key) and <code>COL_VALUE</code>. <seealso cref="COL_VALUE"/>
            /// </summary>
            public const string COL_ID = "id";
            /// <summary>
            /// Column name 
            /// 
            /// Simple relationship table has only two columns: <code>COL_ID</code> or <code>COL_KEY</code> 
            /// (for string key) and <code>COL_VALUE</code>
            /// <seealso cref="COL_VALUE"/>
            /// </summary>
            public const string COL_KEY = "key";
            /// <summary>
            /// Simple relationship table has only two columns: <code>COL_ID</code> or <code>COL_KEY</code> 
            /// (for string key) and <code>COL_VALUE</code>. Sometimes all three columns are used. 
            /// <seealso cref="COL_ID"/> <seealso cref="COL_KEY"/>
            /// </summary>
            public const string COL_VALUE = "value";

            #region INSDC
            /// <summary>
            /// Featured sequence record table for GenBank, DDBJ and EMBL
            /// </summary>
            public const string TABLE_INSDC_KEY = "insid_key";
            /// <summary>
            /// Accession for INSDC record. Same as <code>TABLE_ACCESSION</code>
            /// </summary>
            public const string TABLE_INSDC_ACCESSION = TABLE_ACCESSION;
            public const string TABLE_INSDC_FEATURE = "feature";
            public const string TABLE_INSDC_FEATURE_QUALIFIER = "feature_qualifier";
            public const string TABLE_INSDC_SEQUENCE = TABLE_SEQUENCE;
            public const string COL_FEATURE_ID = "feature_id";
            public const string COL_FEATURE_KEY = COL_KEY;
            public const string COL_FEATURE_LOCATION = COL_VALUE;
            public const string COL_QUALIFIER_ID = "qualifier_id";
            public const string COL_QUALIFIER_KEY = COL_KEY;
            public const string COL_QUALIFIER_VALUE = COL_VALUE;
            #endregion INSDC


            /// <summary>
            /// Column name in <code>TABLE_MASTER</code> for reference index
            /// </summary>
            public const string COL_REF_ID = "ref_id";
            /// <summary>
            /// Column name in <code>TABLE_MASTER</code> for row index
            /// </summary>
            public const string COL_PARENT_FOLDER_ID = "folder_id";
            /// <summary>
            /// Column name in <code>TABLE_MASTER</code> for row index
            /// </summary>
            /// public const string COL_SEQRECORD_ID = "seqrecord_id";
            /// <summary>
            /// Root folder id
            /// </summary>
            public const long ROOT_FOLDER_ID = 0;
            /// <summary>
            /// Default folder id
            /// </summary>
            public long DEFAULT_FOLDER_ID = 1;
            /// <summary>
            /// Folder relationship in <code>TABLE_REL_FOLDER</code>
            /// </summary>
            public const string REL_FOLDER = "folder_rel";
            public const string REL_FOLDER_NAME = "folder_name_rel"; // must be deleted

            public const string SUMMARY_VERSION = "version";


            // members
            List<SourceRecord> listItems = new List<SourceRecord>();
            /// <summary>
            /// Temporary storage for list view
            /// </summary>
            public List<SourceRecord> ListItems { get { return listItems; } }

            protected internal SourceRecord root_folder;
            protected internal SourceRecord default_folder;

            protected internal DataSet dataset;

            public RecordDataSource()
            {
                dataset = new DataSet();

                #region SUMMARY TABLE
                // SUMMARY TABLE
                // Information about this dataset and versioning information
                DataTable tbl = new DataTable("summary");
                DataColumn col = tbl.Columns.Add("key", typeof(string));
                col.Unique = true;
                tbl.Columns.Add("value", typeof(string));
                DataRow row = tbl.NewRow();
                row["key"] = "application";
                row["value"] = "molbio";
                tbl.Rows.Add(row);
                row = tbl.NewRow();
                row["key"] = "data";
                row["value"] = "RecordDatabaseSource";
                tbl.Rows.Add(row);
                row = tbl.NewRow();
                row["key"] = SUMMARY_VERSION;
                row["value"] = "0.1";
                tbl.Rows.Add(row);
                #endregion

                #region BASE TABLES
                // MASTER TABLE
                tbl = new DataTable(TABLE_MASTER);
                DataColumn master_id_col = tbl.Columns.Add(COL_ID, typeof(long));
                master_id_col.AutoIncrement = true;
                master_id_col.AutoIncrementSeed = 0;
                master_id_col.Unique = true;
                master_id_col.ReadOnly = true;
                tbl.PrimaryKey = new DataColumn[] { master_id_col };
                col = tbl.Columns.Add(COL_REF_ID, typeof(long));
                DataColumn name_col = tbl.Columns.Add(COL_NAME, typeof(string));
                DataColumn type_col = tbl.Columns.Add(COL_TYPE, typeof(int));
                type_col.DefaultValue = DataTypes.Unknown;
                DataRow row_default_folder = tbl.NewRow(); // must have root folder
                row_default_folder[COL_ID] = ROOT_FOLDER_ID;
                row_default_folder[COL_NAME] = "root";
                row_default_folder[COL_TYPE] = DataTypes.Folder;
                tbl.Rows.Add(row_default_folder);
                dataset.Tables.Add(tbl);

                // FOLDER TABLE
                tbl = new DataTable(TABLE_REL_FOLDER);
                DataColumn folder_col = tbl.Columns.Add(COL_ID, typeof(long));
                folder_col.AllowDBNull = false;
                DataColumn parent_folder_id_col = tbl.Columns.Add(COL_PARENT_FOLDER_ID, typeof(long));
                parent_folder_id_col.AllowDBNull = false;
                col.DefaultValue = DEFAULT_FOLDER_ID;
                tbl.PrimaryKey = new DataColumn[] { folder_col };
                dataset.Tables.Add(tbl);

                // RELATIONSHIPS
                dataset.Relations.Add(
                    new DataRelation(REL_FOLDER,
                        dataset.Tables[TABLE_MASTER].Columns[COL_ID],
                        dataset.Tables[TABLE_REL_FOLDER].Columns[COL_PARENT_FOLDER_ID])
                    );
                #endregion

                #region  SEQUENCE TABLE
                tbl = new DataTable(TABLE_SEQUENCE);
                DataColumn seq_col = tbl.Columns.Add(COL_ID, typeof(long));
                seq_col.Unique = true;
                seq_col.AllowDBNull = false;
                col = tbl.Columns.Add(COL_VALUE, typeof(string));
                tbl.PrimaryKey = new DataColumn[] { seq_col };
                dataset.Tables.Add(tbl);
                #endregion

                #region INSID
                // INSID TABLES
                tbl = new DataTable(TABLE_INSDC_KEY);
                col = tbl.Columns.Add(COL_ID, typeof(long));
                col.Unique = false;
                col.AllowDBNull = false;
                tbl.PrimaryKey = new DataColumn[] { col };
                col = tbl.Columns.Add(COL_KEY, typeof(string));
                col = tbl.Columns.Add(COL_VALUE, typeof(string));
                dataset.Tables.Add(tbl);

                tbl = new DataTable(TABLE_INSDC_FEATURE);
                col = tbl.Columns.Add(COL_FEATURE_ID, typeof(long));
                col.AutoIncrement = true;
                col.AutoIncrementSeed = 0;
                col.Unique = true;
                col.AllowDBNull = false;
                tbl.PrimaryKey = new DataColumn[] { col };
                col = tbl.Columns.Add(COL_ID, typeof(long));
                col.AllowDBNull = false;
                col = tbl.Columns.Add(COL_FEATURE_KEY, typeof(string));
                col = tbl.Columns.Add(COL_FEATURE_LOCATION, typeof(string));
                dataset.Tables.Add(tbl);

                tbl = new DataTable(TABLE_INSDC_FEATURE_QUALIFIER);
                col = tbl.Columns.Add(COL_QUALIFIER_ID, typeof(long));
                col.AutoIncrement = true;
                col.AutoIncrementSeed = 0;
                col.Unique = true;
                tbl.PrimaryKey = new DataColumn[] { col };
                col = tbl.Columns.Add(COL_FEATURE_ID, typeof(long));
                col.AllowDBNull = false;
                col = tbl.Columns.Add(COL_QUALIFIER_KEY, typeof(string));
                col = tbl.Columns.Add(COL_QUALIFIER_VALUE, typeof(string));
                dataset.Tables.Add(tbl);



                #endregion

                root_folder = new SourceRecord(this, ROOT_FOLDER_ID);

            }

            public ObservableCollection<SourceRecord> Root
            {
                get
                {
                    return root_folder.Children;
                }
            }

            public DataSet DataSet
            {
                get { return dataset; }
            }

                        /// <summary>
            /// Add new record to the root of dataset.
            /// 
            /// To save permently, call <code>Update</code>
            /// </summary>
            /// <param name="record"></param>
            /// <param name="saveNow">set <code>true</code> for immediate save to file or update to database</param>
            /// <returns>true upon update success</returns>
            public bool AddRecord(SeqRecord record)
            {
                // TODO: robust and atomic transction

                long id = -1;
                //// create entry
                DataRow entryRow = dataset.Tables[TABLE_MASTER].NewRow();
                entryRow[COL_NAME] = record.Name;
                entryRow[COL_TYPE] = DataTypes.Sequence;
                // Note: To get id, we have to commit the change
                // in case of error we have to reverse back
                dataset.Tables[TABLE_MASTER].Rows.Add(entryRow);
                id = entryRow.Field<long>(COL_ID);

                DataRow seqRow = dataset.Tables[TABLE_SEQUENCE].NewRow();
                seqRow[COL_ID] = id;
                seqRow[COL_VALUE] = record.Sequence;
                dataset.Tables[TABLE_SEQUENCE].Rows.Add(seqRow);

                return false;
            }

            /// <summary>
            /// Add new record to the root of dataset.
            /// 
            /// To save permently, call <code>Update</code>
            /// </summary>
            /// <param name="record"></param>
            /// <param name="saveNow">set <code>true</code> for immediate save to file or update to database</param>
            /// <returns>true upon update success</returns>
            public bool AddRecord(InsdcRecord record)
            {
                // TODO: robust and atomic transction

                long id = -1;
                //// create entry
                DataRow entryRow = dataset.Tables[TABLE_MASTER].NewRow();
                entryRow[COL_NAME] = record.Name;
                entryRow[COL_TYPE] = DataTypes.Insdc;
                // Note: To get id, we have to commit the change
                // in case of error we have to reverse back
                dataset.Tables[TABLE_MASTER].Rows.Add(entryRow);
                id = entryRow.Field<long>(COL_ID);

                DataRow seqRow = dataset.Tables[TABLE_SEQUENCE].NewRow();
                seqRow[COL_ID] = id;
                seqRow[COL_VALUE] = record.Sequence;
                dataset.Tables[TABLE_SEQUENCE].Rows.Add(seqRow);

                foreach (string key in InsdcRecord.KEYS)
                {
                    if (!record.Keys.ContainsKey(key))
                        continue;
                    DataRow row = dataset.Tables[TABLE_INSDC_KEY].NewRow();
                    row[COL_ID] = id;
                    row[COL_KEY] = key;
                    string value;
                    if (record.Keys.TryGetValue(key, out value))
                        row[COL_VALUE] = value;
                    dataset.Tables[TABLE_INSDC_KEY].Rows.Add(row);
                }

                foreach (Feature ft in record.Features)
                {
                    DataRow featureRow = dataset.Tables[TABLE_INSDC_FEATURE].NewRow();
                    featureRow[COL_ID] = id;
                    featureRow[COL_FEATURE_KEY] = ft.Key;
                    featureRow[COL_FEATURE_LOCATION] = ft.Location;
                    dataset.Tables[TABLE_INSDC_FEATURE].Rows.Add(featureRow);
                    long ft_id = featureRow.Field<long>(COL_FEATURE_ID);
                    foreach (string qualifier in ft.Qualifiers.Keys)
                    {
                        DataRow qRow = dataset.Tables[TABLE_INSDC_FEATURE_QUALIFIER].NewRow();
                        qRow[COL_FEATURE_ID] = ft_id;
                        qRow[COL_KEY] = qualifier;
                        string value;
                        if (ft.Qualifiers.TryGetValue(qualifier, out value))
                            qRow[COL_VALUE] = value;
                        dataset.Tables[TABLE_INSDC_FEATURE_QUALIFIER].Rows.Add(qRow);
                    }
                }

                //// update relationship
                DataRow rel_row = dataset.Tables[TABLE_REL_FOLDER].NewRow();
                rel_row[COL_ID] = id;
                rel_row[COL_PARENT_FOLDER_ID] = DEFAULT_FOLDER_ID;
                dataset.Tables[TABLE_REL_FOLDER].Rows.Add(rel_row);


                //// commit change

                //// Update UI
                foreach (SourceRecord item in ListItems)
                {
                    if (item.Id == DEFAULT_FOLDER_ID)
                    {
                        item.Children.Add(new SourceRecord(this, id));
                        break;
                    }
                }


                return true;
            }

            /// <summary>
            /// Update the changes
            /// </summary>
            /// <returns></returns>
            virtual public bool Update()
            {
                return false;
            }

            /// <summary>
            /// Close the data. This will call update
            /// </summary>
            /// <returns></returns>
            virtual public bool Close()
            {
                return Update();
            }

            public static RecordDataSource FromSeqRecord(SeqRecord[] records)
            {
                // this code is under progress
                RecordDataSource source = new RecordDataSource();
                foreach (SeqRecord record in records)
                {
                    source.AddRecord(record);
                }
                return source;
            }

        }

        public class FileRecordDatabaseSource : RecordDataSource
        {
            public const string FILE_EXT = "frds";
            private string fileName;

            /// <summary>
            /// Open record collection file
            /// </summary>
            /// <param name="fileName"></param>
            public FileRecordDatabaseSource(string fileName)
                : base()
            {
                if (File.Exists(fileName))
                {
                    //FileStream fileStream = new FileStream(fileName, FileMode.Open);
                    //BinaryFormatter deserializer = new BinaryFormatter();
                    //dataset = (DataSet)deserializer.Deserialize(fileStream);
                    //fileStream.Close();
                    dataset.ReadXml(fileName);
                }
                else
                {
                    // put default folder
                    DataRow row_default_folder = dataset.Tables[TABLE_MASTER].NewRow(); // must have default folder
                    row_default_folder[COL_ID] = DEFAULT_FOLDER_ID;
                    row_default_folder[COL_NAME] = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    row_default_folder[COL_TYPE] = DataTypes.Folder;
                    dataset.Tables[TABLE_MASTER].Rows.Add(row_default_folder);

                    DataRow row = dataset.Tables[TABLE_REL_FOLDER].NewRow();
                    row[COL_ID] = DEFAULT_FOLDER_ID;
                    row[COL_PARENT_FOLDER_ID] = ROOT_FOLDER_ID;
                    dataset.Tables[TABLE_REL_FOLDER].Rows.Add(row);
                }


                this.fileName = fileName;

                // TODO: we need to read DEFAULT_FOLDER_ID from dataset
                default_folder = new SourceRecord(this, DEFAULT_FOLDER_ID);
            }

            public override bool Update()
            {
                try
                {
                    //IFormatter formatter = new BinaryFormatter();
                    //Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    //formatter.Serialize(stream, dataset);
                    //stream.Close();

                    dataset.WriteXml(fileName);
                }
                catch
                {
                    return false;
                }
                return true;
            }

            override public bool Close()
            {
                return Update();
            }

        }

    }
}
