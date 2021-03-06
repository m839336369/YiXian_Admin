﻿using Make.MODEL.TCP_Async_Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Make.MODEL.RPC
{
    public class RPCRequestProxyFactory
    {
        private static ConcurrentDictionary<Tuple<string, string, string>, object> services { get; } = new ConcurrentDictionary<Tuple<string, string, string>, object>();
        /// <summary>
        /// 获取RPC代理
        /// </summary>
        /// <param name="servicename">服务名</param>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static T Register<T>(string servicename,string hostname, string port)
        {
            T service = default(T);
            Tuple<string, string, string> key = new Tuple<string, string, string>(servicename, hostname, port);
            services.TryGetValue(key,out object obj);
            service = (T)obj;
            if(service == null)
            {
                SocketClient socketClient = null;
                Tuple<string, string> clientkey = new Tuple<string, string>(hostname, port);
                try
                {
                    socketClient = RPCClientFactory.GetClient(clientkey);
                    service = RPCRequestProxy<T>.Create(servicename, new Tuple<string, string>(hostname,port));
                    services[key] = service;
                }
                catch (SocketException err)
                {
                    if(socketClient == null)
                    {
                        socketClient = RPCClientFactory.GetClient(clientkey);
                    }
                    else socketClient.Reconnect();
                }
            }
            return service;
        }
        public static void Destory(Tuple<string, string, string> key)
        {
            services.TryRemove(key, out object value);
            bool flag = false;
            foreach(Tuple<string,string,string> item in services.Keys)
            {
                if (item.Item2 == key.Item2 && item.Item3 == key.Item3) flag = true;
            }
            if (flag == false) RPCClientFactory.Destory(new Tuple<string, string>(key.Item2,key.Item3));
        }
    }
}
