using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using PortableDeviceApiLib;
using PortableDeviceTypesLib;
using _tagpropertykey = PortableDeviceApiLib._tagpropertykey;
using IPortableDeviceKeyCollection = PortableDeviceApiLib.IPortableDeviceKeyCollection;
using IPortableDeviceValues = PortableDeviceApiLib.IPortableDeviceValues;

namespace ElectricPowerDebuger.Common
{

    class WpdHelper
    {
        private PortableDeviceManager deviceManager;
        private PortableDevice portableDevice;
        private IPortableDeviceContent deviceContent;
        private IPortableDeviceProperties deviceProperties;
        private IPortableDeviceValues deviceValues;

        private static string eventCookie = string.Empty;

        public delegate void WpdEvent();
        public static WpdEvent UnexpectedClosed;

        public WpdHelper()
        {
            
        }

        #region 定义常用属性键/事件ID
        /// <summary>
        /// 操作wpd常用属性键/事件ID，参考 C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Include\PortableDevice.h
        /// </summary>
        public struct pKey
        {
            // client property
            public static _tagpropertykey WPD_CLIENT_NAME                   = DEFINE_PROPERTYKEY(WPD_CLIENT_NAME, 0x204D9F0C, 0x2292, 0x4080, 0x9F, 0x42, 0x40, 0x66, 0x4E, 0x70, 0xF8, 0x59, 2); 
            public static _tagpropertykey WPD_CLIENT_MAJOR_VERSION          = DEFINE_PROPERTYKEY( WPD_CLIENT_MAJOR_VERSION , 0x204D9F0C, 0x2292, 0x4080, 0x9F, 0x42, 0x40, 0x66, 0x4E, 0x70, 0xF8, 0x59 , 3 ); 
            public static _tagpropertykey WPD_CLIENT_MINOR_VERSION          = DEFINE_PROPERTYKEY( WPD_CLIENT_MINOR_VERSION , 0x204D9F0C, 0x2292, 0x4080, 0x9F, 0x42, 0x40, 0x66, 0x4E, 0x70, 0xF8, 0x59 , 4 );
            public static _tagpropertykey WPD_CLIENT_REVISION               = DEFINE_PROPERTYKEY(WPD_CLIENT_REVISION, 0x204D9F0C, 0x2292, 0x4080, 0x9F, 0x42, 0x40, 0x66, 0x4E, 0x70, 0xF8, 0x59, 5); 

            // object property
            public static _tagpropertykey WPD_OBJECT_ID                     = DEFINE_PROPERTYKEY(WPD_OBJECT_ID, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 2);
            public static _tagpropertykey WPD_OBJECT_PARENT_ID              = DEFINE_PROPERTYKEY(WPD_OBJECT_PARENT_ID, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 3);
            public static _tagpropertykey WPD_OBJECT_NAME                   = DEFINE_PROPERTYKEY(WPD_OBJECT_NAME, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 4);
            public static _tagpropertykey WPD_OBJECT_PERSISTENT_UNIQUE_ID   = DEFINE_PROPERTYKEY(WPD_OBJECT_PERSISTENT_UNIQUE_ID, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 5);
            public static _tagpropertykey WPD_OBJECT_FORMAT                 = DEFINE_PROPERTYKEY(WPD_OBJECT_FORMAT, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 6);
            public static _tagpropertykey WPD_OBJECT_CONTENT_TYPE           = DEFINE_PROPERTYKEY(WPD_OBJECT_CONTENT_TYPE, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 7); 
            public static _tagpropertykey WPD_OBJECT_SIZE                   = DEFINE_PROPERTYKEY(WPD_OBJECT_SIZE, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 11);
            public static _tagpropertykey WPD_OBJECT_ORIGINAL_FILE_NAME     = DEFINE_PROPERTYKEY(WPD_OBJECT_ORIGINAL_FILE_NAME, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 12);
            public static _tagpropertykey WPD_OBJECT_REFERENCES             = DEFINE_PROPERTYKEY(WPD_OBJECT_REFERENCES, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 14);
            public static _tagpropertykey WPD_OBJECT_KEYWORDS               = DEFINE_PROPERTYKEY(WPD_OBJECT_KEYWORDS, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 15);
            public static _tagpropertykey WPD_OBJECT_SYNC_ID                = DEFINE_PROPERTYKEY(WPD_OBJECT_SYNC_ID, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 16); 
            public static _tagpropertykey WPD_OBJECT_DATE_CREATED           = DEFINE_PROPERTYKEY(WPD_OBJECT_DATE_CREATED, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 18); 
            public static _tagpropertykey WPD_OBJECT_DATE_MODIFIED          = DEFINE_PROPERTYKEY(WPD_OBJECT_DATE_MODIFIED, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 19);
            public static _tagpropertykey WPD_OBJECT_DATE_AUTHORED          = DEFINE_PROPERTYKEY(WPD_OBJECT_DATE_AUTHORED, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 20);
            public static _tagpropertykey WPD_OBJECT_CAN_DELETE             = DEFINE_PROPERTYKEY(WPD_OBJECT_CAN_DELETE, 0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C, 26); 
            
            // storage property
            public static _tagpropertykey WPD_STORAGE_TYPE                  = DEFINE_PROPERTYKEY(WPD_STORAGE_TYPE, 0x01A3057A, 0x74D6, 0x4E80, 0xBE, 0xA7, 0xDC, 0x4C, 0x21, 0x2C, 0xE5, 0x0A, 2);
            public static _tagpropertykey WPD_STORAGE_FILE_SYSTEM_TYPE      = DEFINE_PROPERTYKEY(WPD_STORAGE_FILE_SYSTEM_TYPE, 0x01A3057A, 0x74D6, 0x4E80, 0xBE, 0xA7, 0xDC, 0x4C, 0x21, 0x2C, 0xE5, 0x0A, 3);
            public static _tagpropertykey WPD_STORAGE_CAPACITY              = DEFINE_PROPERTYKEY(WPD_STORAGE_CAPACITY, 0x01A3057A, 0x74D6, 0x4E80, 0xBE, 0xA7, 0xDC, 0x4C, 0x21, 0x2C, 0xE5, 0x0A, 4);
            public static _tagpropertykey WPD_STORAGE_FREE_SPACE_IN_BYTES   = DEFINE_PROPERTYKEY(WPD_STORAGE_FREE_SPACE_IN_BYTES, 0x01A3057A, 0x74D6, 0x4E80, 0xBE, 0xA7, 0xDC, 0x4C, 0x21, 0x2C, 0xE5, 0x0A, 5);

            // device property
            public static _tagpropertykey WPD_DEVICE_SYNC_PARTNER           = DEFINE_PROPERTYKEY(WPD_DEVICE_SYNC_PARTNER, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 2);
            public static _tagpropertykey WPD_DEVICE_FIRMWARE_VERSION       = DEFINE_PROPERTYKEY(WPD_DEVICE_FIRMWARE_VERSION, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 3);
            public static _tagpropertykey WPD_DEVICE_POWER_LEVEL            = DEFINE_PROPERTYKEY(WPD_DEVICE_POWER_LEVEL, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 4);
            public static _tagpropertykey WPD_DEVICE_MANUFACTURER           = DEFINE_PROPERTYKEY(WPD_DEVICE_MANUFACTURER, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 7);
            public static _tagpropertykey WPD_DEVICE_MODEL                  = DEFINE_PROPERTYKEY(WPD_DEVICE_MODEL, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 8);
            public static _tagpropertykey WPD_DEVICE_SERIAL_NUMBER          = DEFINE_PROPERTYKEY(WPD_DEVICE_SERIAL_NUMBER, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 9);
            public static _tagpropertykey WPD_DEVICE_DATETIME               = DEFINE_PROPERTYKEY(WPD_DEVICE_DATETIME, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 11);
            public static _tagpropertykey WPD_DEVICE_FRIENDLY_NAME          = DEFINE_PROPERTYKEY(WPD_DEVICE_FRIENDLY_NAME, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 12);
            public static _tagpropertykey WPD_DEVICE_TYPE                   = DEFINE_PROPERTYKEY(WPD_DEVICE_TYPE, 0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC, 15);

            // resource property
            public static _tagpropertykey WPD_RESOURCE_DEFAULT              = DEFINE_PROPERTYKEY(WPD_RESOURCE_DEFAULT, 0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F, 0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42, 0);
            public static _tagpropertykey WPD_RESOURCE_GENERIC              = DEFINE_PROPERTYKEY(WPD_RESOURCE_GENERIC, 0xB9B9F515, 0xBA70, 0x4647, 0x94, 0xDC, 0xFA, 0x49, 0x25, 0xE9, 0x5A, 0x07, 0);

            // event property
            public static _tagpropertykey WPD_EVENT_PARAMETER_PNP_DEVICE_ID = DEFINE_PROPERTYKEY(WPD_EVENT_PARAMETER_PNP_DEVICE_ID, 0x15AB1953, 0xF817, 0x4FEF, 0xA9, 0x21, 0x56, 0x76, 0xE8, 0x38, 0xF6, 0xE0, 2);
            public static _tagpropertykey WPD_EVENT_PARAMETER_EVENT_ID      = DEFINE_PROPERTYKEY(WPD_EVENT_PARAMETER_EVENT_ID, 0x15AB1953, 0xF817, 0x4FEF, 0xA9, 0x21, 0x56, 0x76, 0xE8, 0x38, 0xF6, 0xE0, 3);


            private static _tagpropertykey DEFINE_PROPERTYKEY(_tagpropertykey pkeyIgnore, uint l, ushort w1, ushort w2, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, uint pid)
            {
                return new _tagpropertykey() { fmtid = new Guid(l, w1, w2, b1, b2, b3, b4, b5, b6, b7, b8), pid = pid };
            }
        }

        public struct pGuid
        {
            // event id
            public static Guid WPD_EVENT_OBJECT_ADDED               = DEFINE_GUID(WPD_EVENT_OBJECT_ADDED, 0xA726DA95, 0xE207, 0x4B02, 0x8D, 0x44, 0xBE, 0xF2, 0xE8, 0x6C, 0xBF, 0xFC);
            public static Guid WPD_EVENT_OBJECT_REMOVED             = DEFINE_GUID(WPD_EVENT_OBJECT_REMOVED, 0xBE82AB88, 0xA52C, 0x4823, 0x96, 0xE5, 0xD0, 0x27, 0x26, 0x71, 0xFC, 0x38);
            public static Guid WPD_EVENT_OBJECT_UPDATED             = DEFINE_GUID(WPD_EVENT_OBJECT_UPDATED, 0x1445A759, 0x2E01, 0x485D, 0x9F, 0x27, 0xFF, 0x07, 0xDA, 0xE6, 0x97, 0xAB);
            public static Guid WPD_EVENT_DEVICE_RESET               = DEFINE_GUID(WPD_EVENT_DEVICE_RESET, 0x7755CF53, 0xC1ED, 0x44F3, 0xB5, 0xA2, 0x45, 0x1E, 0x2C, 0x37, 0x6B, 0x27);
            public static Guid WPD_EVENT_DEVICE_REMOVED             = DEFINE_GUID(WPD_EVENT_DEVICE_REMOVED, 0xE4CBCA1B, 0x6918, 0x48B9, 0x85, 0xEE, 0x02, 0xBE, 0x7C, 0x85, 0x0A, 0xF9);

            // content type
            public static Guid WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT   = DEFINE_GUID(WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT, 0x99ED0160, 0x17FF, 0x4C44, 0x9D, 0x98, 0x1D, 0x7A, 0x6F, 0x94, 0x19, 0x21);
            public static Guid WPD_CONTENT_TYPE_FOLDER              = DEFINE_GUID(WPD_CONTENT_TYPE_FOLDER, 0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C, 0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            public static Guid WPD_CONTENT_TYPE_GENERIC_FILE        = DEFINE_GUID(WPD_CONTENT_TYPE_GENERIC_FILE, 0x0085E0A6, 0x8D34, 0x45D7, 0xBC, 0x5C, 0x44, 0x7E, 0x59, 0xC7, 0x3D, 0x48);

            // object format
            public static Guid WPD_OBJECT_FORMAT_PROPERTIES_ONLY    = DEFINE_GUID(WPD_OBJECT_FORMAT_PROPERTIES_ONLY, 0x30010000, 0xAE6C, 0x4804, 0x98, 0xBA, 0xC5, 0x7B, 0x46, 0x96, 0x5F, 0xE7);
            public static Guid WPD_OBJECT_FORMAT_UNSPECIFIED        = DEFINE_GUID(WPD_OBJECT_FORMAT_UNSPECIFIED, 0x30000000, 0xAE6C, 0x4804, 0x98, 0xBA, 0xC5, 0x7B, 0x46, 0x96, 0x5F, 0xE7);
            public static Guid WPD_OBJECT_FORMAT_MICROSOFT_EXCEL    = DEFINE_GUID(WPD_OBJECT_FORMAT_MICROSOFT_EXCEL, 0xBA850000, 0xAE6C, 0x4804, 0x98, 0xBA, 0xC5, 0x7B, 0x46, 0x96, 0x5F, 0xE7);


            private static Guid DEFINE_GUID(Guid nameIgnore, uint l, ushort w1, ushort w2, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
            {
                return new Guid(l, w1, w2, b1, b2, b3, b4, b5, b6, b7, b8);
            }
        }
        
        #endregion

        #region 获取连接的设备ID
        /// <summary>
        /// 枚举所有便携式设备（MTP模式）
        /// </summary>
        /// <returns>返回设备id数组</returns>
        public string[] EnumerateDeviceIds()
        {
            string[] deviceIds = null;
            uint deviceCount = 1;   //设备数目初始值必须大于0

            deviceManager = new PortableDeviceManager();
            deviceManager.GetDevices(null, ref deviceCount);    //获取设备数目必须置第一个参数为null
            if (deviceCount > 0)
            {
                deviceIds = new string[deviceCount];
                deviceManager.GetDevices(ref deviceIds[0], ref deviceCount);
            }

            return deviceIds;
        }

        
        public string GetFriendlyName(string deviceID)
        {
            string name = string.Empty;
            uint nameLength = 20;
            ushort[] nameBuffer = new ushort[nameLength];

            deviceManager = new PortableDeviceManager();
            deviceManager.GetDeviceFriendlyName(deviceID, ref nameBuffer[0], ref nameLength);

            for (int i = 0; i < nameLength;  i++ )
            {
                if (nameBuffer[i] != 0)
                    name += (char)nameBuffer[i];
            }

            return name;
        }

        /// <summary>
        /// 获取设备ID
        /// </summary>
        /// 不指定设备名时，获取第一个设备的id 
        /// <returns></returns>
        public string GetDeviceId(string deviceName = "")
        {
            string deviceID = string.Empty;

            string[] ids = EnumerateDeviceIds();
            if(ids != null)
            {
                foreach(string id in ids)
                {
                    if(GetFriendlyName(id).ToLower().Contains(deviceName.ToLower()))
                    {
                        deviceID = id;
                        break;
                    }
                }
            }

            return deviceID;
        }
        #endregion

        /// <summary>
        /// 连接设备
        /// </summary>
        public bool Connect(string DeviceId)
        {
            bool bRet = true;

            try
            {
                IPortableDeviceValues clientInfo = (IPortableDeviceValues)new PortableDeviceTypesLib.PortableDeviceValues();
                portableDevice = new PortableDevice();
                portableDevice.Open(DeviceId, clientInfo);
                portableDevice.Content(out deviceContent);
                deviceContent.Properties(out deviceProperties);
                deviceProperties.GetValues("DEVICE", null, out deviceValues);

                AdviceEvent();
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }
        /// <summary>
        /// 断开设备
        /// </summary>
        /// <param name="portableDevice"></param>
        public void Disconnect()
        {
            try
            {
                if (eventCookie != "")
                {
                    UnadviseEvent();
                }
                eventCookie = "";
                portableDevice.Close();
            }
            catch (Exception) { }
        }

        #region 设备事件
        private void AdviceEvent()
        {
            if (eventCookie != "") return;

            DeviceEventCallback callback = new DeviceEventCallback();

            portableDevice.Advise(0, callback, null, out eventCookie);
        }

        private void UnadviseEvent()
        {
            if (eventCookie == "") return;

            portableDevice.Unadvise(eventCookie);

            eventCookie = "";
        }
        #endregion

        #region 参考demo
        public string Demo_getDeviceInfo()
        {
            string info = "";

            //string deviceId_KT50_B2 = "\\\\?\\usb#vid_0e8d&pid_2008#0123456789abcdef#{6ac27878-a6fa-4155-ba85-f98f491d4f33}";
            //string deviceId_HUAWEI_P10 = "\\\\?\\usb#vid_12d1&pid_107e&mi_00#6&10069075&0&0000#{6ac27878-a6fa-4155-ba85-f98f491d4f33}";

            WpdHelper wpd = new WpdHelper();
            string deviceId = wpd.GetDeviceId();

            if (wpd.Connect(deviceId))
            {
                List<string> storagesIds = wpd.GetChildrenObjectIds("DEVICE");
                ulong freeSpace = 0;
                ulong storageCapacity = 0;

                //设备支持WIFI则包含网络这个假设备，也是无法获取到容量的
                foreach (string storageId in storagesIds)
                {
                    if (storageId == "s10001")
                    {
                        wpd.GetStorageCapacityAnFreeSpace(storageId, out freeSpace, out storageCapacity);
                        break;
                    }
                }

                info += string.Format("设备名称：{0}\r\n", wpd.GetDeviceName());
                info += string.Format("设备型号：{0}\r\n", wpd.GetModel());
                info += string.Format("设备类型：{0}\r\n", wpd.GetDeviceType());
                info += string.Format("设备厂商：{0}\r\n", wpd.GetManufacturer());
                info += string.Format("固件版本：{0}\r\n", wpd.GetFirmwareVersion());
                info += string.Format("存储空间：可用{0}GB / 总共{1}GB\r\n",
                        Math.Round((double)freeSpace / (1000 * 1000 * 1000), 3),
                        Math.Round((double)storageCapacity / (1000 * 1000 * 1000), 3));
            }
            wpd.Disconnect();

            return info;
        }

        public string Demo_DirAndReadFile(string fromFile, string toFile)
        {
            string info = "";

            WpdHelper wpd = new WpdHelper();
            string deviceId = wpd.GetDeviceId();
            if (wpd.Connect(deviceId))
            {
                List<string> storageIds = wpd.GetChildrenObjectIds("DEVICE");
                string findObjName = "";
                string findObjId = "";
                string objName = "";

                foreach (string storageId in storageIds)
                {
                    objName = wpd.GetObjectName(storageId);
                    info += string.Format("storage name：{0}\r\n", objName);

                    List<string> objectIds = wpd.GetChildrenObjectIds(storageId);

                    foreach (string objId in objectIds)
                    {
                        objName = wpd.GetObjectFileName(objId);
                        info += string.Format("object name：{0}\r\n", objName);

                        if (fromFile == objName)
                        {
                            findObjId = objId;
                            findObjName = objName;
                        }
                    }
                }

                if (findObjId != "")
                {
                    info += string.Format("dest read name：{0}\r\n", findObjName);
                }

                bool isOk = wpd.ReadFile(fromFile, toFile);
                info += (isOk ? "read success\r\n" : "read failed\r\n");
            }
            wpd.Disconnect();

            return info;
        }
        public string Demo_DirAndWriteFile(string fromFile, string toPath)
        {
            string info = "";

            WpdHelper wpd = new WpdHelper();
            string deviceId = wpd.GetDeviceId();
            if (wpd.Connect(deviceId))
            {
                List<string> storageIds = wpd.GetChildrenObjectIds("DEVICE");
                string findObjName = "";
                string findObjId = "";
                string objName = "";

                foreach (string storageId in storageIds)
                {
                    objName = wpd.GetObjectName(storageId);
                    info += string.Format("storage name：{0}\r\n", objName);

                    List<string> objectIds = wpd.GetChildrenObjectIds(storageId);

                    foreach (string objId in objectIds)
                    {
                        objName = wpd.GetObjectFileName(objId);
                        info += string.Format("object name：{0}\r\n", objName);

                        if (fromFile == objName)
                        {
                            findObjId = objId;
                            findObjName = objName;
                        }
                    }
                }

                if (toPath == "root" || toPath == "/")
                {
                    findObjId = storageIds[0];
                    findObjName = wpd.GetObjectName(storageIds[0]);
                }

                if (findObjId != "")
                {
                    info += string.Format("dest write name：{0}\r\n", findObjName);
                }

                bool isOk = wpd.WriteFile(fromFile, toPath);
                info += (isOk ? "write success\r\n" : "write failed\r\n");
            }
            wpd.Disconnect();

            return info;
        }

        public string Demo_DeleteFile(string fileName)
        {
            string info = "";
            WpdHelper wpd = new WpdHelper();
            string deviceId = wpd.GetDeviceId();

            if (wpd.Connect(deviceId))
            {
                bool isOk = wpd.DeleteFile(fileName);
                info += (isOk ? "delete success\r\n" : "delete failed\r\n");
            }
            wpd.Disconnect();

            return info;
        }
        #endregion

        #region 获取便携设备信息
        /// <summary>
        /// 设备类型
        /// </summary>
        public enum DeviceType
        {
            Generic = 0,
            Camera = 1,
            MediaPlayer = 2,
            Phone = 3,
            Video = 4,
            PersonalInformationManager = 5,
            AudioRecorder = 6
        };
        /// <summary>
        /// 获取设备类型
        /// </summary>
        public DeviceType GetDeviceType()
        {
            uint propertyValue;
            deviceValues.GetUnsignedIntegerValue(ref pKey.WPD_DEVICE_TYPE, out propertyValue);
            DeviceType deviceType = (DeviceType)propertyValue;
            return deviceType;
        }
        /// <summary>
        /// 获取设备名
        /// </summary>
        public string GetDeviceName()
        {
            string name;
            deviceValues.GetStringValue(ref pKey.WPD_DEVICE_FRIENDLY_NAME, out name);
            return name;
        }
        /// <summary>
        /// 获取固件版本
        /// </summary>
        public string GetFirmwareVersion()
        {
            string firmwareVersion;
            deviceValues.GetStringValue(ref pKey.WPD_DEVICE_FIRMWARE_VERSION, out firmwareVersion);
            return firmwareVersion;
        }
        /// <summary>
        /// 获取制造商
        /// </summary>
        public string GetManufacturer()
        {
            string manufacturer;
            deviceValues.GetStringValue(ref pKey.WPD_DEVICE_MANUFACTURER, out manufacturer);
            return manufacturer;
        }
        /// <summary>
        /// 获取型号
        /// </summary>
        public string GetModel()
        {
            string model;
            deviceValues.GetStringValue(ref pKey.WPD_DEVICE_MODEL, out model);
            return model;
        }

        /// <summary>
        /// 获取设备或设备下文件夹的所有对象（文件、文件夹）的ObjectId
        /// </summary>
        public List<string> GetChildrenObjectIds(string parentObjectId)
        {
            IEnumPortableDeviceObjectIDs objectIds;
            deviceContent.EnumObjects(0, parentObjectId, null, out objectIds);
            List<string> childItems = new List<string>();
            uint fetched = 0;
            do
            {
                string objectId;
                objectIds.Next(1, out objectId, ref fetched);
                if (fetched > 0)
                {
                    childItems.Add(objectId);
                }
            }
            while (fetched > 0);

            return childItems;
        }

        /// <summary>
        /// 获取总容量和可用容量
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="freeSpace"></param>
        /// <param name="storageCapacity"></param>
        public void GetStorageCapacityAnFreeSpace(string storageId, out ulong freeSpace, out ulong storageCapacity)
        {
            IPortableDeviceKeyCollection keyCollection = (IPortableDeviceKeyCollection)new PortableDeviceTypesLib.PortableDeviceKeyCollection();
            keyCollection.Add(ref pKey.WPD_STORAGE_FREE_SPACE_IN_BYTES);
            keyCollection.Add(ref pKey.WPD_STORAGE_CAPACITY);

            IPortableDeviceValues deviceValues;
            deviceProperties.GetValues(storageId, keyCollection, out deviceValues);

            deviceValues.GetUnsignedLargeIntegerValue(ref pKey.WPD_STORAGE_FREE_SPACE_IN_BYTES, out freeSpace);
            deviceValues.GetUnsignedLargeIntegerValue(ref pKey.WPD_STORAGE_CAPACITY, out storageCapacity);
        }

        /// <summary>
        /// 获取对象名
        /// </summary>
        public string GetObjectName(IPortableDeviceValues objectValues)
        {
            string objectName;
            objectValues.GetStringValue(ref pKey.WPD_OBJECT_NAME, out objectName);
            return objectName;
        }
        public string GetObjectName(string objectId)
        {
            IPortableDeviceValues objectValues;
            deviceProperties.GetValues(objectId, null, out objectValues);

            string objectName;
            objectValues.GetStringValue(ref pKey.WPD_OBJECT_NAME, out objectName);
            return objectName;
        }
        /// <summary>
        /// 获取对象文件名
        /// </summary>
        public string GetObjectFileName(string objectId)
        {
            IPortableDeviceValues objectValues;
            deviceProperties.GetValues(objectId, null, out objectValues);

            string fileName;
            objectValues.GetStringValue(ref pKey.WPD_OBJECT_ORIGINAL_FILE_NAME, out fileName);
            return fileName;
        }

        /// <summary>
        /// 获取对象修改时间
        /// </summary>
        public string GetObjectModifiedTime(string objectId)
        {
            IPortableDeviceValues objectValues;
            deviceProperties.GetValues(objectId, null, out objectValues);

            string fileName;
            objectValues.GetStringValue(ref pKey.WPD_OBJECT_DATE_MODIFIED, out fileName);
            return fileName;
        }

        /// <summary>
        /// 获取对象属性:对象名、独立id、文件名、文件大小、引用、关键字、同步id、修改时间
        /// </summary>
        public string GetObjectProperties(string objectId)
        {
            IPortableDeviceValues objectValues;
            deviceProperties.GetValues(objectId, null, out objectValues);

            string properties = "";
            string val = "";

            try
            {
                objectValues.GetStringValue(ref pKey.WPD_OBJECT_ID, out val);
                properties += "obj id:" + val + "\r\n";
                objectValues.GetStringValue(ref pKey.WPD_OBJECT_PARENT_ID, out val);
                properties += "parent id:" + val + "\r\n";
                objectValues.GetStringValue(ref pKey.WPD_OBJECT_NAME, out val);
                properties += "obj name:" + val + "\r\n";
                objectValues.GetStringValue(ref pKey.WPD_OBJECT_PERSISTENT_UNIQUE_ID, out val);
                properties += "unique id:" + val + "\r\n";
                objectValues.GetStringValue(ref pKey.WPD_OBJECT_ORIGINAL_FILE_NAME, out val);
                properties += "file name:" + val + "\r\n";
                objectValues.GetStringValue(ref pKey.WPD_OBJECT_SIZE, out val);
                properties += "file size:" + val + "\r\n";
                //objectValues.GetStringValue(ref pKey.WPD_OBJECT_REFERENCES, out val);
                //properties += "obj ref:" + val + "\r\n";
                //objectValues.GetStringValue(ref pKey.WPD_OBJECT_KEYWORDS, out val);
                //properties += "obj keyword:" + val + "\r\n";
                //objectValues.GetStringValue(ref pKey.WPD_OBJECT_SYNC_ID, out val);
                //properties += "sync id:" + val + "\r\n";
                objectValues.GetStringValue(ref pKey.WPD_OBJECT_DATE_MODIFIED, out val);
                properties += "modify time:" + val + "\r\n";
            }catch(Exception)
            {

            }

            return properties;
        }


        /// <summary>
        /// 在根目录获取文件的对象id
        /// </summary>
        /// <param name="fileName">[/]fileName </param>
        /// <returns>对象id</returns>
        public string GetObjectIdOnRoot(string fileName)
        {
            IPortableDeviceValues values;
            string findObjName = "";
            string findObjId = "";
            List<string> storageIds = GetChildrenObjectIds("DEVICE");

            foreach (string storageId in storageIds)
            {
                if (fileName == "/" || fileName == "root")
                {
                    findObjId = storageId;
                    break;
                }

                List<string> objectIds = GetChildrenObjectIds(storageId);

                foreach (string objId in objectIds)
                {
                    deviceProperties.GetValues(objId, null, out values);
                    values.GetStringValue(ref pKey.WPD_OBJECT_ORIGINAL_FILE_NAME, out findObjName);

                    if (fileName == findObjName)
                    {
                        findObjId = objId;
                        break;
                    }
                }

                if (findObjId != "")
                {
                    break;
                }
            }

            return findObjId;
        }
        /// <summary>
        /// 获取文件的对象id
        /// </summary>
        /// <param name="fileName">[dir/]fileName  如：/dir1/dir2/1.txt </param>
        /// <returns>对象id</returns>
        public string GetObjectId(string fileName)
        {
            IPortableDeviceValues values;
            string findObjName = "";
            string findObjId = "";

            if (string.IsNullOrEmpty(fileName)) return findObjId;

            string[] dirPart = fileName.Split('/');

            List<string> storageIds = GetChildrenObjectIds("DEVICE");

            foreach (string storageId in storageIds)
            {
                if (fileName == "/" || fileName == "root")
                {
                    findObjId = storageId;
                    break;
                }

                List<string> objectIds = GetChildrenObjectIds(storageId);

                foreach (string objId in objectIds)
                {
                    deviceProperties.GetValues(objId, null, out values);
                    values.GetStringValue(ref pKey.WPD_OBJECT_ORIGINAL_FILE_NAME, out findObjName);

                    if (fileName.Contains(findObjName))
                    {
                        findObjId = objId;
                        break;
                    }
                }

                if (findObjId != "")
                {
                    break;
                }
            }

            return findObjId;
        }
        /// <summary>
        /// 获取父对象下所有子对象的id和文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>返回字典[id, fileName]</returns>
        public Dictionary<string, string> EnumObjectIdAndNames(string parentId)
        {
            Dictionary<string, string> dicIdAndName = new Dictionary<string, string>();
            IPortableDeviceValues values;
            string objFileName = "";

            List<string> objectIds = GetChildrenObjectIds(parentId);

            foreach (string objId in objectIds)
            {
                deviceProperties.GetValues(objId, null, out values);
                values.GetStringValue(ref pKey.WPD_OBJECT_ORIGINAL_FILE_NAME, out objFileName);
                dicIdAndName.Add(objId, objFileName);
            }

            return dicIdAndName;
        }
        #endregion

        #region 便携设备写入文件
        /// <summary>
        /// 创建符合要求的文件
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private static IPortableDeviceValues GetRequiredPropertiesForContentType(string sourcePath, string parentId)
        {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;

            values.SetStringValue(ref pKey.WPD_OBJECT_PARENT_ID, parentId);

            values.SetStringValue(ref pKey.WPD_OBJECT_NAME, Path.GetFileName(sourcePath));

            FileInfo fileInfo = new FileInfo(sourcePath);
            values.SetUnsignedLargeIntegerValue(ref pKey.WPD_OBJECT_SIZE, (ulong)fileInfo.Length);

            values.SetStringValue(ref pKey.WPD_OBJECT_ORIGINAL_FILE_NAME, Path.GetFileName(sourcePath));

            return values;
        }

        /// <summary>
        /// 创建符合要求的文件夹
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        private IPortableDeviceValues GetRequiredPropertiesForFolder(string parentId, string folderName)
        {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;

            values.SetStringValue(ref pKey.WPD_OBJECT_PARENT_ID, parentId);

            values.SetStringValue(ref pKey.WPD_OBJECT_NAME, folderName);

            values.SetStringValue(ref pKey.WPD_OBJECT_ORIGINAL_FILE_NAME, folderName);

            values.SetGuidValue(ref pKey.WPD_OBJECT_CONTENT_TYPE, ref pGuid.WPD_CONTENT_TYPE_FOLDER);

            values.SetGuidValue(ref pKey.WPD_OBJECT_FORMAT, ref pGuid.WPD_OBJECT_FORMAT_PROPERTIES_ONLY);

            return values;
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="portableDevice"></param>
        /// <param name="parentId"></param>
        public string CreateDirectoryOnDevice(string folderName, string parentId)
        {
            string newFolderId = "";
            IPortableDeviceValues values = GetRequiredPropertiesForFolder(parentId, folderName);
            this.deviceContent.CreateObjectWithPropertiesOnly(values, ref newFolderId);

            return newFolderId;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="fromFile"></param>
        /// <param name="toPath"></param>
        public bool WriteFile(string fromFile, string toPath, bool isOverwrite = true)
        {
            bool bRet = false;
            string fileObjId = GetObjectIdOnRoot(Path.GetFileName(fromFile));

            if(fileObjId != "" && isOverwrite)
            {
                bRet = DeleteContentFromDevice(fileObjId);
            }

            if (File.Exists(fromFile))
            {
                string rootObjId = GetObjectIdOnRoot("/");
                bRet = TransferContentToDevice(fromFile, rootObjId);
            }

            return bRet;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="parentId"></param>
        public bool TransferContentToDevice(string sourceFile, string parentId)
        {
            bool bRet = true;
            IPortableDeviceValues values = GetRequiredPropertiesForContentType(sourceFile, parentId);
            PortableDeviceApiLib.IStream tempStream = null;
            uint optimalTransferSize = 0;

            try
            {
                deviceContent.CreateObjectWithPropertiesAndData(values, out tempStream, ref optimalTransferSize, null);
                System.Runtime.InteropServices.ComTypes.IStream targetStream = (System.Runtime.InteropServices.ComTypes.IStream)tempStream;

                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                {
                    int filesize = (int)optimalTransferSize;
                    int bytesRead = 0;
                    IntPtr pcbWritten = IntPtr.Zero;
                    do
                    {
                        byte[] buffer = new byte[filesize];
                        bytesRead = sourceStream.Read(buffer, 0, filesize);
                        targetStream.Write(buffer, bytesRead, pcbWritten);
                    }
                    while (bytesRead > 0);
                }
                targetStream.Commit(0);
            }
            catch(Exception)
            {
                bRet = false;
            }
            finally
            {
                Marshal.ReleaseComObject(tempStream);
            }

            return bRet;
        }

        
        #endregion

        #region 便携设备读出文件

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fromFile"></param>
        /// <param name="toFile"></param>
        /// <returns></returns>
        public bool ReadFile(string fromFile, string toFile, bool isReadNew = false)
        {
            bool bRet = false;

            string fileObjId = GetObjectIdOnRoot(fromFile);

            if (fileObjId != "")
            {
                bRet = TransferContentFromDevice(toFile, fileObjId, isReadNew);
            }

            return bRet;
        }

        private bool TransferContentFromDevice(string toFile, string objectId, bool isReadNew = false)
        {
            bool bRet = true;
            string oldFile = Path.GetDirectoryName(toFile)  + "\\" + Path.GetFileNameWithoutExtension(toFile) + "_old" + Path.GetExtension(toFile);
            FileStream fsOld = null;
            byte[] bufferOld = null;
            int bytesReadOld = 0;
            bool isFileNew = false;

            string folder = Path.GetDirectoryName(toFile);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if(isReadNew)
            {
                if (File.Exists(oldFile))
                {
                    File.Delete(oldFile);
                }

                if (File.Exists(toFile))
                {
                    File.Move(toFile, oldFile);
                }
                else
                {
                    File.Create(oldFile);
                }

                fsOld = new FileStream(oldFile, FileMode.Open, FileAccess.Read);
                bufferOld = new byte[512*1024];
            }

            if (File.Exists(toFile))
            {
                File.Delete(toFile);
            }



            IPortableDeviceResources resources;
            deviceContent.Transfer(out resources);
            PortableDeviceApiLib.IStream wpdStream = null;
            uint optimalTransferSize = int.MaxValue;

            try
            {
                //设备建议读取长度optimalTransferSize长度一般为262144即256k， 
                resources.GetStream(objectId, ref pKey.WPD_RESOURCE_DEFAULT, 0, ref optimalTransferSize, out wpdStream);
                System.Runtime.InteropServices.ComTypes.IStream sourceStream = (System.Runtime.InteropServices.ComTypes.IStream)wpdStream;

                string filename = Path.GetFileName(toFile);
                FileStream targetStream = new FileStream(toFile, FileMode.Create, FileAccess.Write);
                {
                    int filesize = (int)optimalTransferSize;
                    byte[] buffer = new byte[filesize];
                    int bytesRead = 0;
                    IntPtr bytesReadIntPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(bytesRead));

                    do
                    {
                        sourceStream.Read(buffer, filesize, bytesReadIntPtr);
                        bytesRead = Marshal.ReadInt32(bytesReadIntPtr);
                        if (filesize > bytesRead)
                        {
                            filesize = bytesRead;
                        }

                        if (isReadNew && isFileNew == false)
                        {
                            bytesReadOld = fsOld.Read(bufferOld, 0, bytesRead);
                            if (bytesReadOld != bytesRead
                                || IsDataDiff(buffer, bufferOld, bytesRead))
                            {
                                isFileNew = true;
                            }
                        }

                        targetStream.Write(buffer, 0, filesize);

                    } while (bytesRead > 0);

                    targetStream.Close();
                    targetStream.Dispose();
                    

                    Marshal.FreeCoTaskMem(bytesReadIntPtr);
                }
            }
            catch(Exception)
            {
                bRet = false;
            }
            finally
            {
                //若不添此行，在本方法执行一次后再次执行时会报资源占用错误            
                Marshal.ReleaseComObject(wpdStream);

                if (isReadNew)
                {
                    fsOld.Close();
                    File.Delete(oldFile);
                    bRet = (isFileNew == false && bRet == false ? false : true);
                }
            }

            return bRet;
        }

        private bool IsDataDiff(byte[] buf1, byte[] buf2, int cnt)
        {
            bool isDiff = false;

            if (cnt > buf1.Length || cnt > buf2.Length) 
            {
                throw new Exception("compare size can't bigger than buffer size"); 
            }

            for(int i = 0; i < cnt; i++)
            {
                if((buf1[i] ^ buf2[i]) != 0)
                {
                    isDiff = true;
                    break;
                }
            }

            return isDiff;
        }
        #endregion

        #region 便携设备删除文件
        /// <summary>
        /// 删除文件
        /// </summary> 
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool DeleteFile(string fileName)
        {
            bool bRet = false;
            string fileObjId = GetObjectIdOnRoot(fileName);

            if (fileObjId != "")
            {
                bRet = DeleteContentFromDevice(fileObjId);
            }

            return bRet;
        }

        public enum DELETE_OBJECT_OPTIONS
        {
            NO_RECURSION = 0,
            WITH_RECURSION = 1
        };
        public bool DeleteContentFromDevice(string objectId)
        {
            bool bRet = false;
            PortableDeviceApiLib.IPortableDevicePropVariantCollection toDelete =
                new PortableDeviceTypesLib.PortableDevicePropVariantCollection()
                as PortableDeviceApiLib.IPortableDevicePropVariantCollection;
            PortableDeviceApiLib.IPortableDevicePropVariantCollection result =
                new PortableDeviceTypesLib.PortableDevicePropVariantCollection()
                as PortableDeviceApiLib.IPortableDevicePropVariantCollection;

            var pv = new PortableDeviceApiLib.tag_inner_PROPVARIANT();

            StringToPropVariant(objectId, out pv);

            toDelete.Add(ref pv);
            
            try
            {
                deviceContent.Delete((uint)DELETE_OBJECT_OPTIONS.NO_RECURSION, toDelete, ref result);
            }
            catch(Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        private void StringToPropVariant(string objectId, out PortableDeviceApiLib.tag_inner_PROPVARIANT pv)
        {
            IPortableDeviceValues values = (PortableDeviceApiLib.IPortableDeviceValues) new PortableDeviceTypesLib.PortableDeviceValues();

            values.SetStringValue(ref pKey.WPD_OBJECT_ID, objectId);
            values.GetValue(ref pKey.WPD_OBJECT_ID, out pv);
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct PropVariant
        {
            [FieldOffset(0)]
            public short variantType;   // VT_LPWSTR = 31  , VT_LPSTR = 30
            [FieldOffset(8)]
            public IntPtr pointerValue;     
            [FieldOffset(8)]
            public byte byteValue;
            [FieldOffset(8)]
            public long longValue;
        }

        #endregion
    }

    class DeviceEventCallback : IPortableDeviceEventCallback
    {
        static string deviceId = "";
        static string eventId = "";

        public void OnEvent(IPortableDeviceValues pEventParameters)
        {

            pEventParameters.GetStringValue(WpdHelper.pKey.WPD_EVENT_PARAMETER_PNP_DEVICE_ID, out deviceId);
            pEventParameters.GetStringValue(WpdHelper.pKey.WPD_EVENT_PARAMETER_EVENT_ID, out eventId);

            if (eventId.Contains(WpdHelper.pGuid.WPD_EVENT_DEVICE_RESET.ToString().ToUpper())
                || eventId.Contains(WpdHelper.pGuid.WPD_EVENT_DEVICE_REMOVED.ToString().ToUpper()))
            {
                if(WpdHelper.UnexpectedClosed != null)
                {
                    WpdHelper.UnexpectedClosed();
                }
            }
            else
            {
                /*
                if (eventId.Contains(WpdHelper.pGuid.WPD_EVENT_OBJECT_ADDED.ToString().ToUpper()))
                {
                    eventId = "1";
                }
                else if (eventId.Contains(WpdHelper.pGuid.WPD_EVENT_OBJECT_REMOVED.ToString().ToUpper()))
                {
                    eventId = "2";
                }
                else if (eventId.Contains(WpdHelper.pGuid.WPD_EVENT_OBJECT_UPDATED.ToString().ToUpper()))
                {
                    eventId = "3";
                }
                 * */
            }
        }
    }
}
