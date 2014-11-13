using System;
using System.Diagnostics;
using System.Reflection;

namespace Courier
{
	[DebuggerDisplay("MethodInfo.Name: {callbackMethod.Name} WeakReference.Target: {callbackWeakReference.Target}")]
	internal class WeakSubscriber
	{
		private readonly MethodInfo callbackMethod;
		private readonly MethodInfo onErrorMethod;

		public MethodInfo CallbackMethod { get { return callbackMethod; } }
		public MethodInfo OnErrorMethod { get { return onErrorMethod; } } 

		private readonly Type delegateType;
		private readonly WeakReference callbackWeakReference;
		private readonly WeakReference onErrorWeakReference;

		internal WeakSubscriber(Object callbackTarget, MethodInfo callbackMethod, Object onErrorTarget, MethodInfo onErrorMethod, Type parameterType)
		{
			//create a WeakReference to store the instance of the callbackTarget in which the callbackMethod resides
			callbackWeakReference = new WeakReference(callbackTarget);
			this.callbackMethod = callbackMethod;

			onErrorWeakReference = new WeakReference(onErrorTarget);
			this.onErrorMethod = onErrorMethod;

			delegateType = parameterType == null ? typeof(Action) : typeof(Action<>).MakeGenericType(parameterType);
		}

		internal MulticastDelegate CreateCallbackDelegate()
		{
            try
            {
                Object target = callbackWeakReference.Target;
                return target != null ? Delegate.CreateDelegate(delegateType, callbackWeakReference.Target, callbackMethod) as MulticastDelegate : null;

            }
            catch (MemberAccessException)
            {
                return null;
            }
        }

		internal MulticastDelegate CreateOnErrorDelegate()
		{
			try
			{
				Object target = onErrorWeakReference.Target;
				return target != null ? Delegate.CreateDelegate(typeof(Action<Exception>), onErrorWeakReference.Target, onErrorMethod) as MulticastDelegate : null;

			}
			catch (MemberAccessException)
			{
				return null;
			}
		}

		public Boolean IsCallbackAlive
		{
			get { return callbackWeakReference.IsAlive; }
		}

		public Boolean IsOnErrorAlive
		{
			get { return onErrorWeakReference.IsAlive; }
		}
	}
}