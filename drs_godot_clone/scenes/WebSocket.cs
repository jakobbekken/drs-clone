using Godot;
using System;
using System.Text.Json;
using System.Text;
using Game;

namespace Game;
public partial class WebSocket : Node2D
{
    [Export] public int _port = 6969;
    private string _ip_address = "localhost";
    private WebSocketPeer _webSocketPeer;
    private InputData _latestInput;

    // Signal only supports native Godot types, not arbitrary C# classes
    [Signal]
    public delegate void DataReceivedEventHandler(float foot1Pos, int foot1Step, float foot2Pos, int foot2Step);

    public override void _Ready()
    {
        this._webSocketPeer = new WebSocketPeer();
        var error = this._webSocketPeer.ConnectToUrl($"ws://{_ip_address}:{this._port}");
        if (error != Error.Ok)
        {
            GD.Print("==== ERROR ====");
            GD.Print($"Could not bind PacketPeerUdp to designated port {this._port} or IP address {this._ip_address}");
            GD.Print("==== ERROR ====");
            GD.Print("");
        }
        GD.Print("==== SUCCESS ====");
        GD.Print($"Listening on port {this._port} and IP address {this._ip_address}");
    }

    public override void _Process(double delta)
    {
        if (this._webSocketPeer.GetReadyState() == WebSocketPeer.State.Open)
        {
            while (this._webSocketPeer.GetAvailablePacketCount() > 0)
            {
                byte[] packet = this._webSocketPeer.GetPacket();
                string rawPacketString = Encoding.UTF8.GetString(packet);
                try
                {
                    this._latestInput = JsonSerializer.Deserialize<InputData>(rawPacketString);
                }
                catch (JsonException error)
                {
                    GD.Print($"Could not parse JSON: {error}");
                }

                GD.Print("==== WEB-SOCKET RECIEVED DATA =====");
                GD.Print($"Foot 1 position: {this._latestInput.foot0Pos}");
                GD.Print($"Foot 1 state: {this._latestInput.foot0Step}");
                GD.Print($"Foot 2 position: {this._latestInput.foot1Pos}");
                GD.Print($"Foot 2 state: {this._latestInput.foot1Step}");
                EmitSignal(SignalName.DataReceived, this._latestInput.foot0Pos, this._latestInput.foot0Step,
                                                    this._latestInput.foot1Pos, this._latestInput.foot1Step);
            }
        }
    }
}
