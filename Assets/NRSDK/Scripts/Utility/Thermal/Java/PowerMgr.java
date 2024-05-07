package com.nreal.nrealapp;

import android.content.Context;
import android.os.Build;
import android.os.PowerManager;
import android.util.Log;

//import androidx.annotation.RequiresApi;

public class PowerMgr {

    private com.nreal.nrealapp.IThermalLog iThermalLog;

//    @RequiresApi(api = Build.VERSION_CODES.Q)
    public  void init(Context context, IThermalLog thermalLog)
    {
        Context mContext = context;
        iThermalLog = thermalLog;
        PowerManager pm = (PowerManager) mContext.getSystemService(Context.POWER_SERVICE);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            pm.addThermalStatusListener(new PowerManager.OnThermalStatusChangedListener()
            {
                @Override
                public void onThermalStatusChanged(int status)
                {
                    //Log.d(TAG, "onThermalStatusChanged:" + status);
                    iThermalLog.OnThermalLog(status);
                }
            });
        }
    }
}
