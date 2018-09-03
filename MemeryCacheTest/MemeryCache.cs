using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace MemeryCacheTest
{
    public static class MemeryCache
    {
        /// <summary>
        /// 从内存缓存中读取配置。若缓存中不存在，则重新从文件中读取配置，存入缓存
        /// </summary>
        /// <param name="cacheKey">缓存Key</param>
        /// <returns>配置词典</returns>
        public static Dictionary<string, string> GetConfigDictionary(string cacheKey)
        {
            Dictionary<string, string> configs = null;

            //1、获取内存缓存对象
            var cache = MemoryCache.Default;

            //2、通过Key判断缓存中是否已有词典内容（Key在存入缓存时设置）
            if (cache.Contains(cacheKey))
            {
                //3、直接从缓存中读取词典内容
                configs = cache.GetCacheItem(cacheKey).Value as Dictionary<string, string>;
            }
            else
            {
                //3、读取配置文件，组成词典对象，准备放到缓存中
                configs = GetFromXml();

                //4、检查是否读取到配置内容
                if (configs != null)
                {
                    //4、新建一个CacheItemPolicy对象，该对象用于声明配置对象在缓存中的处理策略
                    CacheItemPolicy policy = new CacheItemPolicy();

                    //5、因为配置文件一直需要读取，所以在此设置缓存优先级为不应删除
                    // 实际情况请酌情考虑，同时可以设置AbsoluteExpiration属性指定过期时间
                    policy.Priority = CacheItemPriority.NotRemovable;

                    //6、将词典内容添加到缓存，传入 缓存Key、配置对象、对象策略
                    // Set方法首先会检查Key是否在缓存中存在，如果存在，更新value，不存在则创建新的
                    // 这里先加入缓存再加监视的原因是：在缓存加入时，也会触发监视事件，会导致出错。
                    cache.Set(cacheKey, configs, policy);

                    //7、监视文件需要传入一个IList对象，所以即便只有一个文件也需要新建List对象
                    string applicationPath = AppDomain.CurrentDomain.BaseDirectory;
                    string configPath = applicationPath + "MemeryCacheTest.exe.config";
                    List<string> filePaths = new List<string>() {@"c:\config.xml",configPath };

                    //8、新建一个文件监视器对象，添加对资源文件的监视
                    HostFileChangeMonitor monitor = new HostFileChangeMonitor(filePaths);

                    //9、调用监视器的NotifyOnChanged方法传入发生改变时的回调方法
                    monitor.NotifyOnChanged(new OnChangedCallback((o) =>
                    {
                        cache.Remove(cacheKey);
                    }
                        ));

                    //10、为配置对象的缓存策略加入监视器
                    policy.ChangeMonitors.Add(monitor);
                }
            }
            return configs;
        }

        private static Dictionary<string, string> GetFromXml()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            var value = ConfigurationManager.AppSettings["TimeOut"];
            if (value != null)
                dic.Add("TimeOut", value);
            return dic;
        }
    }
}
