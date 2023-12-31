﻿using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuideMeCadastroTags.Droid
{
    [BroadcastReceiver]
    public class PairingRequestReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == BluetoothDevice.ActionPairingRequest)
            {
                if (intent.Action == BluetoothDevice.ActionPairingRequest)
                {
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    // Use the predetermined passkey
                    int predeterminedPasskey = 123456;
                    device.SetPin(Encoding.UTF8.GetBytes(predeterminedPasskey.ToString()));
                    //device.SetPairingConfirmation(true);    
                    device.CreateBond();
                    InvokeAbortBroadcast();

                    while (device.BondState == Bond.Bonding) ;



                }
            }
        }
    }
}