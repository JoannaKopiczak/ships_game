using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace STATKI_SERWER
{
    public partial class Form1 : Form
    {
        List<Button> player1Position;
        List<Button> player2Position;
        private Socket _serwerSocket, _clientSocket;
        int totalScorePlayer1 = 0;
        int totalScorePlayer2 = 0;
        private byte[] _buffer;
        int totalShipsPlayer2 = 3;
        string text;
        string text2 = "A5";
        int rounds = 10;

        public Form1()
        {
            InitializeComponent();

            loadbuttons();
            attackButton.Enabled = false;
            player1LocationList.Text = null;
            StartServer();
        }

        private void loadbuttons()
        {
            player1Position = new List<Button> { w1, w2, w3, w4, x1, x2, x3, x4, y1, y2, y3, y4, z1, z2, z3, z4 };
            player2Position = new List<Button> { a1, a2, a3, a4, b1, b2, b3, b4, c1, c2, c3, c4, d1, d2, d3, d4 };

            for (int i = 0; i < player1Position.Count; i++)
            {
                player1Position[i].Tag = null;
                player1LocationList.Items.Add(player1Position[i].Text);
            }
        }

        private void StartServer()
        {
            try
            {
                _serwerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serwerSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.105"), 8001));
                _serwerSocket.Listen(1);
                _serwerSocket.BeginAccept(new AsyncCallback(AcceptCallback), _serwerSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {

            try
            {
                _clientSocket = _serwerSocket.EndAccept(ar);
                _buffer = new byte[_clientSocket.ReceiveBufferSize];
                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _clientSocket);
                _serwerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                helpText.Invoke(new Action(() => helpText.Text = "Połączono! Ustaw statki na 3 polach ;)"));
              

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {

                int bytesRead = _clientSocket.EndReceive(ar);
                text = Encoding.ASCII.GetString(_buffer);
                while (bytesRead == 0 && text == text2)
                { return; }

                Array.Resize(ref _buffer, bytesRead);
                text2 = Encoding.ASCII.GetString(_buffer);
                Array.Resize(ref _buffer, _clientSocket.ReceiveBufferSize);



              
                string[] w = text2.Split(' ');
                string first = w[0];
                string second = w[1];
                if (first == "atakklienta")
                {
                    rounds--;
                    
                    int index = 0;
                    while (player2Position[index].Text != second) { index++; }
                    
                    if ((string)player2Position[index].Tag == "player2ship")
                    {
                        byte[] buffer = new byte[20];
                        buffer = Encoding.ASCII.GetBytes("atakklienta " + player2Position[index].Text);
                        _clientSocket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), _clientSocket);
                        totalScorePlayer1++;
                        player1Score.Invoke(new Action(() => player1Score.Text = " " + totalScorePlayer1));
                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));
                        player2Moves.Invoke(new Action(() => player2Moves.Text = player2Position[index].Text));
                        helpText.Invoke(new Action(() => helpText.Text = "Strzelaj!"));
                    }
                    else
                    {
                        byte[] buffer = new byte[20];
                        buffer = Encoding.ASCII.GetBytes("atakklienta " + player2Position[index].Text + "p");
                        _clientSocket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), _clientSocket);
                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));
                        player2Moves.Invoke(new Action(() => player2Moves.Text = player2Position[index].Text));
                        helpText.Invoke(new Action(() => helpText.Text = "Strzelaj!"));
                    }
                    if (rounds < 1 || totalScorePlayer1 > 2 || totalScorePlayer2 > 2)
                    {
                        if (totalScorePlayer1 > totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("PRZEGRAŁEŚ :(((");
                        }

                        if (totalScorePlayer1 == totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("REMIS!");
                        }

                        if (totalScorePlayer2 > totalScorePlayer1)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("WYGRAŁEŚ !!!");
                        }
                    }
                    
                }
                else if (first == "atakserwera")
                {
                    rounds--;
                    
                    int index = 0;
                    while ((player1Position[index].Text != second) && ((player1Position[index].Text + "p") != second)) { index++; }

                    if ((player1Position[index].Text + "p") == second)
                    {

                        player1Position[index].Invoke(new Action(() => player1Position[index].BackgroundImage = Properties.Resources.missIcon));
                        player1Position[index].Invoke(new Action(() => player1Position[index].BackColor = System.Drawing.Color.DarkBlue));
                        player1Position[index].Invoke(new Action(() => player1Position[index].Enabled = false));
                        
                        helpText.Invoke(new Action(() => helpText.Text = "Teraz kolej na przeciwnika"));
                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));

                    }
                    else if (player1Position[index].Text == second)
                    {
                        player1Position[index].Invoke(new Action(() => player1Position[index].BackgroundImage = Properties.Resources.fireIcon));
                        player1Position[index].Invoke(new Action(() => player1Position[index].BackColor = System.Drawing.Color.DarkBlue));
                        player1Position[index].Invoke(new Action(() => player1Position[index].Enabled = false));
                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));
                        helpText.Invoke(new Action(() => helpText.Text = "Teraz kolej na przeciwnika"));
                        totalScorePlayer2++;
                        player2Score.Invoke(new Action(() => player2Score.Text = " " + totalScorePlayer2));
                       
                    }
                    if (rounds < 1 || totalScorePlayer1 > 2 || totalScorePlayer2 > 2)
                    {
                        if (totalScorePlayer1 > totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("PRZEGRAŁEŚ :(((");
                        }

                        if (totalScorePlayer1 == totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("REMIS!");
                        }

                        if (totalScorePlayer2 > totalScorePlayer1)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("WYGRAŁEŚ !!!");
                        }
                    }
                    _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket _clientsocket = (Socket)ar.AsyncState;
                _clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        

        private void Player2PicksPosition(object sender, EventArgs e)
        {
            if (totalShipsPlayer2 > 0)
            {
                var button = (Button)sender;
                button.Enabled = false;
                button.Tag = "player2ship";
                button.BackColor = System.Drawing.Color.Maroon;
                totalShipsPlayer2--;
            }


            if (totalShipsPlayer2 == 0)
            {
                attackButton.Enabled = true;
                attackButton.BackColor = System.Drawing.Color.Maroon;
                helpText.Top = 70;
                helpText.Left = 70;
                helpText.Text = "3) Uważaj! Strzela przeciwnik.";
                
            }
        }

        
        private void Player2AttackPlayer(object sender, EventArgs e)
        {

            if (player1LocationList.Text != " ")
            {
                var attackPos = player1LocationList.Text;
                attackPos = attackPos.ToLower();
                int index = player1Position.FindIndex(w => w.Name == attackPos);
              

                if (player1Position[index].Enabled && rounds > 0)
                {
                    byte[] buffer = Encoding.ASCII.GetBytes("atakserwera " + player1Position[index].Text);

                    _clientSocket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), _clientSocket);
                    
                    _buffer = new byte[20];
                   
                    _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(ReceiveCallback), _clientSocket);

                }

            }
            else
            {
                MessageBox.Show("Wybierz pozycje z listy!");
            }

        }

    }
}
