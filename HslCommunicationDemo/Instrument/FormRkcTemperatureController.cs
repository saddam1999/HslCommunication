﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using System.Threading;
using HslCommunication;
using HslCommunication.Instrument.RKC;
using System.IO.Ports;
using System.Xml.Linq;

namespace HslCommunicationDemo
{
	public partial class FormRkcTemperatureController : HslFormContent
	{
		public FormRkcTemperatureController( )
		{
			InitializeComponent( );
			rkc = new TemperatureController( );
			// omronHostLink.LogNet = new HslCommunication.LogNet.LogNetSingle( "omron.log.txt" );
		}


		private TemperatureController rkc = null;


		private void FormSiemens_Load( object sender, EventArgs e )
		{
			panel2.Enabled = false;

			Language( Program.Language );
			comboBox3.DataSource = SerialPort.GetPortNames( );
			try
			{
				comboBox3.SelectedIndex = 0;
			}
			catch
			{
				comboBox3.Text = "COM3";
			}
			comboBox2.SelectedIndex = 0;
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "RKC CD/CH digital temperature controller";

				label1.Text = "Station:";
				label3.Text = "Parity:";
				button1.Text = "Open";
				button2.Text = "Close";
				label21.Text = "Address:";
				label29.Text = "Com:";
				label28.Text = "BaudRate:";
				label27.Text = "DataBit:";
				label26.Text = "StopBit:";

				label11.Text = "Address:";
				label12.Text = "length:";
				button25.Text = "Bulk Read";
				label13.Text = "Results:";
				label16.Text = "Message:";
				label14.Text = "Results:";
				button26.Text = "Read";

				groupBox3.Text = "Bulk Read test";
				groupBox4.Text = "Message reading test, hex string needs to be filled in";
				groupBox5.Text = "Special function test";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}
		
		#region Connect And Close



		private void button1_Click( object sender, EventArgs e )
		{
			if (!int.TryParse( textBox19.Text, out int baudRate ))
			{
				MessageBox.Show( DemoUtils.BaudRateInputWrong );
				return;
			}

			if (!int.TryParse( textBox18.Text, out int dataBits ))
			{
				MessageBox.Show( DemoUtils.DataBitsInputWrong );
				return;
			}

			if (!int.TryParse( textBox2.Text, out int stopBits ))
			{
				MessageBox.Show( DemoUtils.StopBitInputWrong );
				return;
			}

			if (!byte.TryParse( textBox1.Text, out byte Station ))
			{
				MessageBox.Show( "PLC Station input wrong！" );
				return;
			}

			rkc?.Close( );
			rkc = new TemperatureController( );

			try
			{
				rkc.SerialPortInni( sp =>
				{
					sp.PortName = comboBox3.Text;
					sp.BaudRate = baudRate;
					sp.DataBits = dataBits;
					sp.StopBits = stopBits == 0 ? StopBits.None : (stopBits == 1 ? StopBits.One : StopBits.Two);
					sp.Parity = comboBox2.SelectedIndex == 0 ? Parity.None : (comboBox2.SelectedIndex == 1 ? Parity.Odd : Parity.Even);
				} );
				//yamateke.ByteTransform.DataFormat = (HslCommunication.Core.DataFormat)comboBox1.SelectedItem;

				rkc.Open( );
				rkc.Station = Station;
				button2.Enabled = true;
				button1.Enabled = false;
				panel2.Enabled = true;

				userControlReadWriteOp1.SetReadWriteNet( rkc, "M1", false );
				userControlReadWriteOp1.EnableRKC( );
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			rkc.Close( );
			button2.Enabled = false;
			button1.Enabled = true;
			panel2.Enabled = false;
		}
		
		#endregion

		#region 批量读取测试

		private void button25_Click( object sender, EventArgs e )
		{
			DemoUtils.BulkReadRenderResult( rkc, textBox6, textBox9, textBox10 );
		}


		#endregion

		#region 报文读取测试


		private void button26_Click( object sender, EventArgs e )
		{
			OperateResult<byte[]> read = rkc.ReadFromCoreServer( HslCommunication.BasicFramework.SoftBasic.HexStringToBytes( textBox13.Text ) );
			if (read.IsSuccess)
			{
				textBox11.Text = "Result：" + HslCommunication.BasicFramework.SoftBasic.ByteToHexString( read.Content );
			}
			else
			{
				MessageBox.Show( "Read Failed：" + read.ToMessageShowString( ) );
			}
		}


		#endregion
		
		public override void SaveXmlParameter( XElement element )
		{
			element.SetAttributeValue( DemoDeviceList.XmlCom, comboBox3.Text );
			element.SetAttributeValue( DemoDeviceList.XmlBaudRate, textBox19.Text );
			element.SetAttributeValue( DemoDeviceList.XmlDataBits, textBox18.Text );
			element.SetAttributeValue( DemoDeviceList.XmlStopBit, textBox2.Text );
			element.SetAttributeValue( DemoDeviceList.XmlParity, comboBox2.SelectedIndex );
			element.SetAttributeValue( DemoDeviceList.XmlStation, textBox1.Text );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			comboBox3.Text = element.Attribute( DemoDeviceList.XmlCom ).Value;
			textBox19.Text = element.Attribute( DemoDeviceList.XmlBaudRate ).Value;
			textBox18.Text = element.Attribute( DemoDeviceList.XmlDataBits ).Value;
			textBox2.Text = element.Attribute( DemoDeviceList.XmlStopBit ).Value;
			comboBox2.SelectedIndex = int.Parse( element.Attribute( DemoDeviceList.XmlParity ).Value );
			textBox1.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}
