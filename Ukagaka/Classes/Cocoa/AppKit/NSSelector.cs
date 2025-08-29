using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSSelector
    {
        private readonly Action _action;
        private readonly string _selectorName;
        private readonly object _target;

        // 原有构造函数
        public NSSelector(Action action)
        {
            _action = action;
        }

        // 新增构造函数
        public NSSelector(string selectorName, object target)
        {
            _selectorName = selectorName;
            _target = target;
        }

        public void Invoke()
        {
            if (_action != null)
            {
                _action.Invoke();
            }
            else if (_target != null && !string.IsNullOrEmpty(_selectorName))
            {
                // 使用反射调用目标方法
                var method = _target.GetType().GetMethod(_selectorName);
                if (method != null)
                {
                    method.Invoke(_target, null);
                }
            }
        }

        public string SelectorName => _selectorName;
        public object Target => _target;
    }

}
