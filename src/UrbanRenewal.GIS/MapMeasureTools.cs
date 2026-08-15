using System;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 长度测量：单击加点，双击结束；Esc 取消。
    /// </summary>
    public sealed class MeasureLengthTool : ICommand, ITool
    {
        private IMapControl3 _mapControl;
        private INewLineFeedback _feedback;
        private Action<string> _onResult;

        public MeasureLengthTool(Action<string> onResult)
        {
            _onResult = onResult;
        }

        public void OnCreate(object hook)
        {
            _mapControl = hook as IMapControl3;
        }

        public void OnClick()
        {
        }

        public bool Enabled
        {
            get { return _mapControl != null; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public string Name
        {
            get { return "UrbanRenewal_MeasureLength"; }
        }

        public string Caption
        {
            get { return "长度测量"; }
        }

        public string Tooltip
        {
            get { return "单击加点，双击结束测量长度"; }
        }

        public string Message
        {
            get { return Tooltip; }
        }

        public int Bitmap
        {
            get { return 0; }
        }

        public string Category
        {
            get { return "UrbanRenewal"; }
        }

        public string HelpFile
        {
            get { return string.Empty; }
        }

        public int HelpContextID
        {
            get { return 0; }
        }

        public int Cursor
        {
            get { return 0; }
        }

        public bool Deactivate()
        {
            CancelFeedback();
            return true;
        }

        public void OnDblClick()
        {
            if (_feedback == null || _mapControl == null)
            {
                return;
            }

            IPolyline line = _feedback.Stop() as IPolyline;
            _feedback = null;
            if (line == null || line.IsEmpty)
            {
                return;
            }

            double length = line.Length;
            string msg = "测量长度: " + length.ToString("0.##") + " （地图单位）";
            if (_onResult != null)
            {
                _onResult(msg);
            }
            PartialRefresh();
        }

        public bool OnContextMenu(int x, int y)
        {
            return false;
        }

        public void OnKeyDown(int keyCode, int shift)
        {
            // Esc
            if (keyCode == 27)
            {
                CancelFeedback();
                PartialRefresh();
            }
        }

        public void OnKeyUp(int keyCode, int shift)
        {
        }

        public void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1 || _mapControl == null || _mapControl.ActiveView == null)
            {
                return;
            }

            IPoint pt = ToMapPoint(x, y);
            if (pt == null)
            {
                return;
            }

            if (_feedback == null)
            {
                _feedback = new NewLineFeedbackClass();
                _feedback.Display = _mapControl.ActiveView.ScreenDisplay;
                _feedback.Start(pt);
            }
            else
            {
                _feedback.AddPoint(pt);
            }
        }

        public void OnMouseMove(int button, int shift, int x, int y)
        {
            if (_feedback == null)
            {
                return;
            }
            IPoint pt = ToMapPoint(x, y);
            if (pt != null)
            {
                _feedback.MoveTo(pt);
            }
        }

        public void OnMouseUp(int button, int shift, int x, int y)
        {
        }

        public void Refresh(int hdc)
        {
        }

        public void OnRefresh(int hdc)
        {
        }

        private IPoint ToMapPoint(int x, int y)
        {
            try
            {
                return _mapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            }
            catch
            {
                return null;
            }
        }

        private void CancelFeedback()
        {
            if (_feedback != null)
            {
                try { _feedback.Stop(); }
                catch { }
                _feedback = null;
            }
        }

        private void PartialRefresh()
        {
            try
            {
                if (_mapControl != null && _mapControl.ActiveView != null)
                {
                    _mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                }
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// 面积测量：单击加点，双击结束；Esc 取消。
    /// </summary>
    public sealed class MeasureAreaTool : ICommand, ITool
    {
        private IMapControl3 _mapControl;
        private INewPolygonFeedback _feedback;
        private Action<string> _onResult;

        public MeasureAreaTool(Action<string> onResult)
        {
            _onResult = onResult;
        }

        public void OnCreate(object hook)
        {
            _mapControl = hook as IMapControl3;
        }

        public void OnClick()
        {
        }

        public bool Enabled
        {
            get { return _mapControl != null; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public string Name
        {
            get { return "UrbanRenewal_MeasureArea"; }
        }

        public string Caption
        {
            get { return "面积测量"; }
        }

        public string Tooltip
        {
            get { return "单击加点，双击结束测量面积"; }
        }

        public string Message
        {
            get { return Tooltip; }
        }

        public int Bitmap
        {
            get { return 0; }
        }

        public string Category
        {
            get { return "UrbanRenewal"; }
        }

        public string HelpFile
        {
            get { return string.Empty; }
        }

        public int HelpContextID
        {
            get { return 0; }
        }

        public int Cursor
        {
            get { return 0; }
        }

        public bool Deactivate()
        {
            CancelFeedback();
            return true;
        }

        public void OnDblClick()
        {
            if (_feedback == null || _mapControl == null)
            {
                return;
            }

            IPolygon polygon = _feedback.Stop() as IPolygon;
            _feedback = null;
            if (polygon == null || polygon.IsEmpty)
            {
                return;
            }

            try
            {
                polygon.Close();
            }
            catch
            {
            }

            IArea area = polygon as IArea;
            double a = area != null ? Math.Abs(area.Area) : 0;
            string msg = "测量面积: " + a.ToString("0.##") + " （地图单位²）";
            if (_onResult != null)
            {
                _onResult(msg);
            }
            PartialRefresh();
        }

        public bool OnContextMenu(int x, int y)
        {
            return false;
        }

        public void OnKeyDown(int keyCode, int shift)
        {
            if (keyCode == 27)
            {
                CancelFeedback();
                PartialRefresh();
            }
        }

        public void OnKeyUp(int keyCode, int shift)
        {
        }

        public void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1 || _mapControl == null || _mapControl.ActiveView == null)
            {
                return;
            }

            IPoint pt = ToMapPoint(x, y);
            if (pt == null)
            {
                return;
            }

            if (_feedback == null)
            {
                _feedback = new NewPolygonFeedbackClass();
                _feedback.Display = _mapControl.ActiveView.ScreenDisplay;
                _feedback.Start(pt);
            }
            else
            {
                _feedback.AddPoint(pt);
            }
        }

        public void OnMouseMove(int button, int shift, int x, int y)
        {
            if (_feedback == null)
            {
                return;
            }
            IPoint pt = ToMapPoint(x, y);
            if (pt != null)
            {
                _feedback.MoveTo(pt);
            }
        }

        public void OnMouseUp(int button, int shift, int x, int y)
        {
        }

        public void Refresh(int hdc)
        {
        }

        public void OnRefresh(int hdc)
        {
        }

        private IPoint ToMapPoint(int x, int y)
        {
            try
            {
                return _mapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            }
            catch
            {
                return null;
            }
        }

        private void CancelFeedback()
        {
            if (_feedback != null)
            {
                try { _feedback.Stop(); }
                catch { }
                _feedback = null;
            }
        }

        private void PartialRefresh()
        {
            try
            {
                if (_mapControl != null && _mapControl.ActiveView != null)
                {
                    _mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                }
            }
            catch
            {
            }
        }
    }
}
