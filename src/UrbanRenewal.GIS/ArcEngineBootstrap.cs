using System;
using System.Windows.Forms;
using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// ArcGIS Engine / Desktop 运行时绑定与许可初始化。
    /// </summary>
    public static class ArcEngineBootstrap
    {
        private static IAoInitialize _aoInit;
        private static bool _initialized;

        public static bool IsInitialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// 绑定运行时并初始化许可。须在创建任何 AO 对象之前调用。
        /// </summary>
        public static bool TryInitialize(out string message)
        {
            message = null;
            if (_initialized)
            {
                return true;
            }

            try
            {
                // 避免 PATH 中 64 位 GDAL 插件被 x86 ArcEngine 加载导致崩溃
                try
                {
                    Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", "");
                    Environment.SetEnvironmentVariable("GDAL_DATA", "");
                }
                catch
                {
                }

                if (!RuntimeManager.Bind(ProductCode.EngineOrDesktop))
                {
                    message = "无法绑定 ArcGIS Runtime（Engine/Desktop）。请确认已安装 ArcGIS 10.2。";
                    return false;
                }

                _aoInit = new AoInitializeClass();
                esriLicenseStatus status = TryProduct(_aoInit, esriLicenseProductCode.esriLicenseProductCodeEngine);
                if (status != esriLicenseStatus.esriLicenseCheckedOut)
                {
                    status = TryProduct(_aoInit, esriLicenseProductCode.esriLicenseProductCodeBasic);
                }
                if (status != esriLicenseStatus.esriLicenseCheckedOut)
                {
                    status = TryProduct(_aoInit, esriLicenseProductCode.esriLicenseProductCodeStandard);
                }
                if (status != esriLicenseStatus.esriLicenseCheckedOut)
                {
                    status = TryProduct(_aoInit, esriLicenseProductCode.esriLicenseProductCodeAdvanced);
                }

                if (status != esriLicenseStatus.esriLicenseCheckedOut)
                {
                    message = "ArcGIS 许可签出失败，状态: " + status;
                    return false;
                }

                // 动力性分析需要 Spatial Analyst（缓冲转栅格、像元统计、栅格计算）
                esriLicenseProductCode usedProduct = esriLicenseProductCode.esriLicenseProductCodeEngine;
                // 以最后成功签出的产品为准：再次探测可用产品
                if (_aoInit.IsProductCodeAvailable(esriLicenseProductCode.esriLicenseProductCodeAdvanced) == esriLicenseStatus.esriLicenseAvailable
                    || _aoInit.InitializedProduct() == esriLicenseProductCode.esriLicenseProductCodeAdvanced)
                {
                    usedProduct = _aoInit.InitializedProduct();
                }
                else
                {
                    usedProduct = _aoInit.InitializedProduct();
                }

                esriLicenseStatus ext = _aoInit.IsExtensionCodeAvailable(
                    usedProduct,
                    esriLicenseExtensionCode.esriLicenseExtensionCodeSpatialAnalyst);
                if (ext == esriLicenseStatus.esriLicenseAvailable)
                {
                    ext = _aoInit.CheckOutExtension(esriLicenseExtensionCode.esriLicenseExtensionCodeSpatialAnalyst);
                }
                bool hasSa = ext == esriLicenseStatus.esriLicenseCheckedOut;

                // 路网可达性需要 Network Analyst（枚举名在 10.2 为 Network；无许可时引擎回退欧氏距离）
                esriLicenseStatus naExt = _aoInit.IsExtensionCodeAvailable(
                    usedProduct,
                    esriLicenseExtensionCode.esriLicenseExtensionCodeNetwork);
                if (naExt == esriLicenseStatus.esriLicenseAvailable)
                {
                    naExt = _aoInit.CheckOutExtension(esriLicenseExtensionCode.esriLicenseExtensionCodeNetwork);
                }
                bool hasNa = naExt == esriLicenseStatus.esriLicenseCheckedOut;

                _initialized = true;
                if (hasSa && hasNa)
                {
                    message = "ArcGIS 许可初始化成功（含 Spatial Analyst、Network Analyst）。";
                }
                else if (hasSa)
                {
                    message = "ArcGIS 许可初始化成功（含 Spatial Analyst；Network Analyst 不可用，路网可达性将回退欧氏近似）。";
                }
                else
                {
                    message = "ArcGIS 主许可已签出，但 Spatial Analyst 不可用（状态: " + ext
                        + "）。动力性栅格分析可能失败。Network Analyst: " + naExt + "。";
                }
                return true;
            }
            catch (Exception ex)
            {
                message = "ArcGIS 初始化异常: " + ex.Message;
                return false;
            }
        }

        public static void Shutdown()
        {
            if (_aoInit != null)
            {
                try
                {
                    _aoInit.Shutdown();
                }
                catch
                {
                }
                _aoInit = null;
            }
            _initialized = false;
        }

        private static esriLicenseStatus TryProduct(IAoInitialize ao, esriLicenseProductCode product)
        {
            esriLicenseStatus status = ao.IsProductCodeAvailable(product);
            if (status == esriLicenseStatus.esriLicenseAvailable)
            {
                status = ao.Initialize(product);
            }
            return status;
        }
    }
}
