using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Demo3
{
    public static class DataManager
    {
        public static List<Order> Orders = new List<Order>();
        public static List<Station> Stns1 = new List<Station>();
        public static List<Station> Stns2 = new List<Station>();
        public static List<Train> Trains = new List<Train>();
        public static List<Train> OnlineTrains = new List<Train>();
        public static int nowRollingStockNum = 0;
        public static Dictionary<int, int> Dic4RollingStock = new Dictionary<int, int>();
        public static void Clear()
        {
            Orders.Clear();
            Stns1.Clear();
            Stns2.Clear();
            Trains.Clear();
            OnlineTrains.Clear();
            nowRollingStockNum = 0;
            Dic4RollingStock.Clear();
        }

        public static void SaveFileTest(string fileName)
        {
            TrainGraphRoot tg = new TrainGraphRoot() { TrainGraph = new TrainGraph() };
            tg.TrainGraph.ServerNumber = new ServerNumber[nowRollingStockNum];
            for (int i = 0; i < nowRollingStockNum; ++i)
            {
                tg.TrainGraph.ServerNumber[i] = new ServerNumber();
                tg.TrainGraph.ServerNumber[i].ID = (ushort)0;
                tg.TrainGraph.ServerNumber[i].TrainNumber = new TrainNumber[Dic4RollingStock[i]];
            }
            foreach (var train in Trains)
            {
                var server = tg.TrainGraph.ServerNumber[train.RollingStockNum];
                if (train.Arrs.Count == 0) continue;
                train.dir = train.Arrs.Last().Key.StnName == "岗厦北站" ? 1 : 0;
                //int id;
                //if (int.TryParse(train.TrainNum, out id))
                //{
                //    id = 999;
                //}
                TrainNumber _train = new TrainNumber()
                {
                    OrderNumber = train.TrainNum,
                    //OrderNumber = (byte)(id),
                    RunDirect = (byte)(train.dir),
                };
                server.TrainNumber[train.OrderNum - 1] = _train;
                _train.Train = new _Train[train.Arrs.Count * 2];
                List<_Train> trains = new List<_Train>();
                bool flag = false;
                List<Station> stns = train.dir == 1 ? DataManager.Stns1 : Stns2;
                foreach (var stn in stns)
                {
                    if (!train.Arrs.ContainsKey(stn)) continue;
                    DateTime arrtime = train.Arrs[stn];
                    DateTime deptime = train.Deps[stn];
                    if (arrtime.Hour < 4)
                    {
                        flag = true;
                        break;
                    }
                    _Train tarr = new _Train();
                    tarr.Time = arrtime.ToString("HH:mm:ss");
                    if (!Test._downDic.ContainsKey(stn.StnName))
                    {
                        continue;
                    }
                    tarr.StopAreaID = (byte)(train.dir == 1 ? Test._downDic[stn.StnName] : Test._upDic[stn.StnName]);
                    tarr.TrainInfoType = 0;
                    _Train tdep = new _Train();
                    tdep.Time = deptime.ToString("HH:mm:ss");
                    tdep.StopAreaID = (byte)(train.dir == 1 ? Test._downDic[stn.StnName] : Test._upDic[stn.StnName]);
                    tdep.TrainInfoType = 1;
                    tarr.IsReturn = "False";
                    tdep.IsReturn = "False";
                    trains.Add(tarr);
                    trains.Add(tdep);
                    if (tdep.StopAreaID == 8 && train.NextTrain==null)
                    {
                        _Train tturn = new _Train();
                        tturn.Time = deptime.AddSeconds(5).ToString("HH:mm:ss");
                        tturn.StopAreaID = 10;
                        trains.Add(tturn);
                    }
                    if (tdep.StopAreaID == 53 && train.NextTrain == null)
                    {
                        _Train tturn = new _Train();
                        tturn.Time = deptime.AddSeconds(5).ToString("HH:mm:ss");
                        tturn.StopAreaID = 55;
                        _Train tturn2 = new _Train();
                        trains.Add(tturn);
                    }
                }
                if (!flag && trains.Count > 0)
                {
                    trains.Last().IsReturn = "True";
                    _train.Train = trains.ToArray();
                }
            }
            using (FileStream fs = new FileStream(Environment.CurrentDirectory + "\\" + fileName + ".xml", FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TrainGraphRoot));
                using StreamWriter sw = new StreamWriter(fs);
                xmlSerializer.Serialize(sw, tg);
            }
        }



        #region xml data format
        // 注意: 生成的代码可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public class TrainGraphRoot
        {

            private TrainGraph trainGraphField;

            /// <remarks/>
            public TrainGraph TrainGraph
            {
                get
                {
                    return this.trainGraphField;
                }
                set
                {
                    this.trainGraphField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class TrainGraph
        {

            private ServerNumber[] serverNumberField;

            private byte idField = 0;

            private string nameField = "";

            private string checkedPassField = "False";

            private byte versionField = 0;

            private string createUserIDField = "";

            private string createTimeField = DateTime.Now.ToString("yyyy/M/d H:mm:ss");

            private string updateUserIDField = "";

            private string updateTimeField = DateTime.Now.ToString("yyyy/M/d H:mm:ss");

            private byte lineIDField = 131;

            private byte typeField = 1;

            private string examineUserIDField = "";

            private byte linkBasicGraphField = 0;

            private byte editTypeField = 0;

            private string isIntegerityField = "False";

            private byte statusField = 1;

            private string linkBasicGraphNameField = "";

            private string statusUpdateTimeField = DateTime.MinValue.ToString("yyyy/M/d H:mm:ss");

            private string statusUpdateUserIDField = "";

            private string signedUserNameField = "";

            private byte creationSourceField = 0;

            private string reserveField = "";

            private decimal fileVersionField = 1.0M;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("ServerNumber")]
            public ServerNumber[] ServerNumber
            {
                get
                {
                    return this.serverNumberField;
                }
                set
                {
                    this.serverNumberField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte ID
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CheckedPass
            {
                get
                {
                    return this.checkedPassField;
                }
                set
                {
                    this.checkedPassField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CreateUserID
            {
                get
                {
                    return this.createUserIDField;
                }
                set
                {
                    this.createUserIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CreateTime
            {
                get
                {
                    return this.createTimeField;
                }
                set
                {
                    this.createTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string UpdateUserID
            {
                get
                {
                    return this.updateUserIDField;
                }
                set
                {
                    this.updateUserIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string UpdateTime
            {
                get
                {
                    return this.updateTimeField;
                }
                set
                {
                    this.updateTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte LineID
            {
                get
                {
                    return this.lineIDField;
                }
                set
                {
                    this.lineIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string ExamineUserID
            {
                get
                {
                    return this.examineUserIDField;
                }
                set
                {
                    this.examineUserIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte LinkBasicGraph
            {
                get
                {
                    return this.linkBasicGraphField;
                }
                set
                {
                    this.linkBasicGraphField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte EditType
            {
                get
                {
                    return this.editTypeField;
                }
                set
                {
                    this.editTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string IsIntegerity
            {
                get
                {
                    return this.isIntegerityField;
                }
                set
                {
                    this.isIntegerityField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Status
            {
                get
                {
                    return this.statusField;
                }
                set
                {
                    this.statusField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string LinkBasicGraphName
            {
                get
                {
                    return this.linkBasicGraphNameField;
                }
                set
                {
                    this.linkBasicGraphNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string StatusUpdateTime
            {
                get
                {
                    return this.statusUpdateTimeField;
                }
                set
                {
                    this.statusUpdateTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string StatusUpdateUserID
            {
                get
                {
                    return this.statusUpdateUserIDField;
                }
                set
                {
                    this.statusUpdateUserIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string SignedUserName
            {
                get
                {
                    return this.signedUserNameField;
                }
                set
                {
                    this.signedUserNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte CreationSource
            {
                get
                {
                    return this.creationSourceField;
                }
                set
                {
                    this.creationSourceField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Reserve
            {
                get
                {
                    return this.reserveField;
                }
                set
                {
                    this.reserveField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal FileVersion
            {
                get
                {
                    return this.fileVersionField;
                }
                set
                {
                    this.fileVersionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class ServerNumber
        {
            private TrainNumber[] trainNumberField;

            private ushort idField;

            private string iSLOCKField = "False";

            private string hasCustomColorField = "False";

            private string customColorInfoField = "";

            private string isCrossLineServerField = "False";

            private string reserveField = "";

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("TrainNumber")]
            public TrainNumber[] TrainNumber
            {
                get
                {
                    return this.trainNumberField;
                }
                set
                {
                    this.trainNumberField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort ID
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string ISLOCK
            {
                get
                {
                    return this.iSLOCKField;
                }
                set
                {
                    this.iSLOCKField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string HasCustomColor
            {
                get
                {
                    return this.hasCustomColorField;
                }
                set
                {
                    this.hasCustomColorField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CustomColorInfo
            {
                get
                {
                    return this.customColorInfoField;
                }
                set
                {
                    this.customColorInfoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string IsCrossLineServer
            {
                get
                {
                    return this.isCrossLineServerField;
                }
                set
                {
                    this.isCrossLineServerField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Reserve
            {
                get
                {
                    return this.reserveField;
                }
                set
                {
                    this.reserveField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class TrainNumber
        {

            private _Train[] trainField;

            private string orderNumberField;

            private byte runDirectField;

            private string isLockField = "False";

            private string hasCustomColorField = "False";

            private string customColorInfoField = "";

            private string isCrossLineField = "False";

            private byte crossLineServerNumField = 0;

            private byte srcLineIdField = 0;

            private byte dstLineIdField = 0;

            private byte crossSrcStopAreaIdField = 0;

            private byte crossDstStopAreaIdField = 0;

            private string crossSrcOrderNumberField = "";

            private string crossDstOrderNumebrField = "";

            private string reserveField = "";

            private byte trainOrganizeField = 0;

            private string stopRuleSignField = "";

            private string runRuleSignField = "";

            private byte passengerStationField = 0;

            private byte lastOrderNumberField;

            private bool lastOrderNumberFieldSpecified;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Train")]
            public _Train[] Train
            {
                get
                {
                    return this.trainField;
                }
                set
                {
                    this.trainField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string OrderNumber
            {
                get
                {
                    return this.orderNumberField;
                }
                set
                {
                    this.orderNumberField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte RunDirect
            {
                get
                {
                    return this.runDirectField;
                }
                set
                {
                    this.runDirectField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string IsLock
            {
                get
                {
                    return this.isLockField;
                }
                set
                {
                    this.isLockField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string HasCustomColor
            {
                get
                {
                    return this.hasCustomColorField;
                }
                set
                {
                    this.hasCustomColorField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CustomColorInfo
            {
                get
                {
                    return this.customColorInfoField;
                }
                set
                {
                    this.customColorInfoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string IsCrossLine
            {
                get
                {
                    return this.isCrossLineField;
                }
                set
                {
                    this.isCrossLineField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte CrossLineServerNum
            {
                get
                {
                    return this.crossLineServerNumField;
                }
                set
                {
                    this.crossLineServerNumField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte SrcLineId
            {
                get
                {
                    return this.srcLineIdField;
                }
                set
                {
                    this.srcLineIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte DstLineId
            {
                get
                {
                    return this.dstLineIdField;
                }
                set
                {
                    this.dstLineIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte CrossSrcStopAreaId
            {
                get
                {
                    return this.crossSrcStopAreaIdField;
                }
                set
                {
                    this.crossSrcStopAreaIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte CrossDstStopAreaId
            {
                get
                {
                    return this.crossDstStopAreaIdField;
                }
                set
                {
                    this.crossDstStopAreaIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CrossSrcOrderNumber
            {
                get
                {
                    return this.crossSrcOrderNumberField;
                }
                set
                {
                    this.crossSrcOrderNumberField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CrossDstOrderNumebr
            {
                get
                {
                    return this.crossDstOrderNumebrField;
                }
                set
                {
                    this.crossDstOrderNumebrField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Reserve
            {
                get
                {
                    return this.reserveField;
                }
                set
                {
                    this.reserveField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte TrainOrganize
            {
                get
                {
                    return this.trainOrganizeField;
                }
                set
                {
                    this.trainOrganizeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string StopRuleSign
            {
                get
                {
                    return this.stopRuleSignField;
                }
                set
                {
                    this.stopRuleSignField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string RunRuleSign
            {
                get
                {
                    return this.runRuleSignField;
                }
                set
                {
                    this.runRuleSignField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte passengerStation
            {
                get
                {
                    return this.passengerStationField;
                }
                set
                {
                    this.passengerStationField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte LastOrderNumber
            {
                get
                {
                    return this.lastOrderNumberField;
                }
                set
                {
                    this.lastOrderNumberField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool LastOrderNumberSpecified
            {
                get
                {
                    return this.lastOrderNumberFieldSpecified;
                }
                set
                {
                    this.lastOrderNumberFieldSpecified = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public class _Train
        {

            private string timeField;

            private byte stopAreaIDField;

            private byte trainInfoTypeField;

            private string isReturnField;

            private string reserveField;

            private string hasCustomColorField = "False";

            private string customColorInfoField = "";

            private byte runLevelField = 1;

            private bool runLevelFieldSpecified;

            private string isCarryField = "True";

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Time
            {
                get
                {
                    return this.timeField;
                }
                set
                {
                    this.timeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte StopAreaID
            {
                get
                {
                    return this.stopAreaIDField;
                }
                set
                {
                    this.stopAreaIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte TrainInfoType
            {
                get
                {
                    return this.trainInfoTypeField;
                }
                set
                {
                    this.trainInfoTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string IsReturn
            {
                get
                {
                    return this.isReturnField;
                }
                set
                {
                    this.isReturnField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Reserve
            {
                get
                {
                    return this.reserveField;
                }
                set
                {
                    this.reserveField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string HasCustomColor
            {
                get
                {
                    return this.hasCustomColorField;
                }
                set
                {
                    this.hasCustomColorField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CustomColorInfo
            {
                get
                {
                    return this.customColorInfoField;
                }
                set
                {
                    this.customColorInfoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte RunLevel
            {
                get
                {
                    return this.runLevelField;
                }
                set
                {
                    this.runLevelField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlIgnoreAttribute()]
            public bool RunLevelSpecified
            {
                get
                {
                    return this.runLevelFieldSpecified;
                }
                set
                {
                    this.runLevelFieldSpecified = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string IsCarry
            {
                get
                {
                    return this.isCarryField;
                }
                set
                {
                    this.isCarryField = value;
                }
            }
        }
        #endregion
    }
}
