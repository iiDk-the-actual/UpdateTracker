using System;
using System.Runtime.InteropServices;
using AOT;
using LitJson;
using Viveport.Core;
using Viveport.Internal.Arcade;

namespace Viveport.Arcade
{
	internal class Session
	{
		[MonoPInvokeCallback(typeof(SessionCallback))]
		private static void IsReadyIl2cppCallback(int errorCode, string message)
		{
			Session.isReadyIl2cppCallback(errorCode, message);
		}

		public static void IsReady(Session.SessionListener listener)
		{
			Session.isReadyIl2cppCallback = new Session.SessionHandler(listener).getIsReadyHandler();
			if (IntPtr.Size == 8)
			{
				Session.IsReady_64(new SessionCallback(Session.IsReadyIl2cppCallback));
				return;
			}
			Session.IsReady(new SessionCallback(Session.IsReadyIl2cppCallback));
		}

		[MonoPInvokeCallback(typeof(SessionCallback))]
		private static void StartIl2cppCallback(int errorCode, string message)
		{
			Session.startIl2cppCallback(errorCode, message);
		}

		public static void Start(Session.SessionListener listener)
		{
			Session.startIl2cppCallback = new Session.SessionHandler(listener).getStartHandler();
			if (IntPtr.Size == 8)
			{
				Session.Start_64(new SessionCallback(Session.StartIl2cppCallback));
				return;
			}
			Session.Start(new SessionCallback(Session.StartIl2cppCallback));
		}

		[MonoPInvokeCallback(typeof(SessionCallback))]
		private static void StopIl2cppCallback(int errorCode, string message)
		{
			Session.stopIl2cppCallback(errorCode, message);
		}

		public static void Stop(Session.SessionListener listener)
		{
			Session.stopIl2cppCallback = new Session.SessionHandler(listener).getStopHandler();
			if (IntPtr.Size == 8)
			{
				Session.Stop_64(new SessionCallback(Session.StopIl2cppCallback));
				return;
			}
			Session.Stop(new SessionCallback(Session.StopIl2cppCallback));
		}

		private static SessionCallback isReadyIl2cppCallback;

		private static SessionCallback startIl2cppCallback;

		private static SessionCallback stopIl2cppCallback;

		private class SessionHandler : Session.BaseHandler
		{
			public SessionHandler(Session.SessionListener cb)
			{
				Session.SessionHandler.listener = cb;
			}

			public SessionCallback getIsReadyHandler()
			{
				return new SessionCallback(this.IsReadyHandler);
			}

			protected override void IsReadyHandler(int code, [MarshalAs(UnmanagedType.LPStr)] string message)
			{
				Logger.Log("[Session IsReadyHandler] message=" + message + ",code=" + code.ToString());
				JsonData jsonData = null;
				try
				{
					jsonData = JsonMapper.ToObject(message);
				}
				catch (Exception ex)
				{
					string text = "[Session IsReadyHandler] exception=";
					Exception ex2 = ex;
					Logger.Log(text + ((ex2 != null) ? ex2.ToString() : null));
				}
				int num = -1;
				string text2 = "";
				string text3 = "";
				if (code == 0 && jsonData != null)
				{
					try
					{
						num = (int)jsonData["statusCode"];
						text2 = (string)jsonData["message"];
					}
					catch (Exception ex3)
					{
						string text4 = "[IsReadyHandler] statusCode, message ex=";
						Exception ex4 = ex3;
						Logger.Log(text4 + ((ex4 != null) ? ex4.ToString() : null));
					}
					Logger.Log("[IsReadyHandler] statusCode =" + num.ToString() + ",errMessage=" + text2);
					if (num == 0)
					{
						try
						{
							text3 = (string)jsonData["appID"];
						}
						catch (Exception ex5)
						{
							string text5 = "[IsReadyHandler] appID ex=";
							Exception ex6 = ex5;
							Logger.Log(text5 + ((ex6 != null) ? ex6.ToString() : null));
						}
						Logger.Log("[IsReadyHandler] appID=" + text3);
					}
				}
				if (Session.SessionHandler.listener != null)
				{
					if (code == 0)
					{
						if (num == 0)
						{
							Session.SessionHandler.listener.OnSuccess(text3);
							return;
						}
						Session.SessionHandler.listener.OnFailure(num, text2);
						return;
					}
					else
					{
						Session.SessionHandler.listener.OnFailure(code, message);
					}
				}
			}

			public SessionCallback getStartHandler()
			{
				return new SessionCallback(this.StartHandler);
			}

			protected override void StartHandler(int code, [MarshalAs(UnmanagedType.LPStr)] string message)
			{
				Logger.Log("[Session StartHandler] message=" + message + ",code=" + code.ToString());
				JsonData jsonData = null;
				try
				{
					jsonData = JsonMapper.ToObject(message);
				}
				catch (Exception ex)
				{
					string text = "[Session StartHandler] exception=";
					Exception ex2 = ex;
					Logger.Log(text + ((ex2 != null) ? ex2.ToString() : null));
				}
				int num = -1;
				string text2 = "";
				string text3 = "";
				string text4 = "";
				if (code == 0 && jsonData != null)
				{
					try
					{
						num = (int)jsonData["statusCode"];
						text2 = (string)jsonData["message"];
					}
					catch (Exception ex3)
					{
						string text5 = "[StartHandler] statusCode, message ex=";
						Exception ex4 = ex3;
						Logger.Log(text5 + ((ex4 != null) ? ex4.ToString() : null));
					}
					Logger.Log("[StartHandler] statusCode =" + num.ToString() + ",errMessage=" + text2);
					if (num == 0)
					{
						try
						{
							text3 = (string)jsonData["appID"];
							text4 = (string)jsonData["Guid"];
						}
						catch (Exception ex5)
						{
							string text6 = "[StartHandler] appID, Guid ex=";
							Exception ex6 = ex5;
							Logger.Log(text6 + ((ex6 != null) ? ex6.ToString() : null));
						}
						Logger.Log("[StartHandler] appID=" + text3 + ",Guid=" + text4);
					}
				}
				if (Session.SessionHandler.listener != null)
				{
					if (code == 0)
					{
						if (num == 0)
						{
							Session.SessionHandler.listener.OnStartSuccess(text3, text4);
							return;
						}
						Session.SessionHandler.listener.OnFailure(num, text2);
						return;
					}
					else
					{
						Session.SessionHandler.listener.OnFailure(code, message);
					}
				}
			}

			public SessionCallback getStopHandler()
			{
				return new SessionCallback(this.StopHandler);
			}

			protected override void StopHandler(int code, [MarshalAs(UnmanagedType.LPStr)] string message)
			{
				Logger.Log("[Session StopHandler] message=" + message + ",code=" + code.ToString());
				JsonData jsonData = null;
				try
				{
					jsonData = JsonMapper.ToObject(message);
				}
				catch (Exception ex)
				{
					string text = "[Session StopHandler] exception=";
					Exception ex2 = ex;
					Logger.Log(text + ((ex2 != null) ? ex2.ToString() : null));
				}
				int num = -1;
				string text2 = "";
				string text3 = "";
				string text4 = "";
				if (code == 0 && jsonData != null)
				{
					try
					{
						num = (int)jsonData["statusCode"];
						text2 = (string)jsonData["message"];
					}
					catch (Exception ex3)
					{
						string text5 = "[StopHandler] statusCode, message ex=";
						Exception ex4 = ex3;
						Logger.Log(text5 + ((ex4 != null) ? ex4.ToString() : null));
					}
					Logger.Log("[StopHandler] statusCode =" + num.ToString() + ",errMessage=" + text2);
					if (num == 0)
					{
						try
						{
							text3 = (string)jsonData["appID"];
							text4 = (string)jsonData["Guid"];
						}
						catch (Exception ex5)
						{
							string text6 = "[StopHandler] appID, Guid ex=";
							Exception ex6 = ex5;
							Logger.Log(text6 + ((ex6 != null) ? ex6.ToString() : null));
						}
						Logger.Log("[StopHandler] appID=" + text3 + ",Guid=" + text4);
					}
				}
				if (Session.SessionHandler.listener != null)
				{
					if (code == 0)
					{
						if (num == 0)
						{
							Session.SessionHandler.listener.OnStopSuccess(text3, text4);
							return;
						}
						Session.SessionHandler.listener.OnFailure(num, text2);
						return;
					}
					else
					{
						Session.SessionHandler.listener.OnFailure(code, message);
					}
				}
			}

			private static Session.SessionListener listener;
		}

		private abstract class BaseHandler
		{
			protected abstract void IsReadyHandler(int code, [MarshalAs(UnmanagedType.LPStr)] string message);

			protected abstract void StartHandler(int code, [MarshalAs(UnmanagedType.LPStr)] string message);

			protected abstract void StopHandler(int code, [MarshalAs(UnmanagedType.LPStr)] string message);
		}

		public class SessionListener
		{
			public virtual void OnSuccess(string pchAppID)
			{
			}

			public virtual void OnStartSuccess(string pchAppID, string pchGuid)
			{
			}

			public virtual void OnStopSuccess(string pchAppID, string pchGuid)
			{
			}

			public virtual void OnFailure(int nCode, string pchMessage)
			{
			}
		}
	}
}
