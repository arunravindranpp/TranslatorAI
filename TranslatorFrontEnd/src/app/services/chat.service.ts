import { Inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { ConfigService } from './config.service';
import { TranslationService } from './translation.service';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private hubConnection: signalR.HubConnection | null = null;
  private chatUrl: string;
  public messages: { username: string; content: string }[] = [];

  constructor(
    @Inject(ConfigService) private configService: ConfigService,
    private translationService: TranslationService // Inject TranslationService here
  ) {
    this.chatUrl = this.configService.get('apiEndpoints').chatUrl;
  }

  startConnection(username: string): Promise<void> {
    const urlWithQuery = `${this.chatUrl}?username=${username}`;
  
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(urlWithQuery, {
        withCredentials: true  // Ensure credentials are sent with the request
      })
      .build();
  
    return this.hubConnection.start()
      .then(() => {
        console.log('SignalR connection established');
      })
      .catch((err) => {
        console.error('Error establishing SignalR connection:', err);
        throw new Error('Error establishing SignalR connection');
      });
  }
  
  getActiveUsers(): Promise<string[]> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return this.hubConnection.invoke('GetActiveUsers');
    } else {
      return Promise.reject(new Error('SignalR connection is not connected.'));
    }
  }
  async sendMessage(username: string, recipient: string, content: string) {
    if(!this.hubConnection )
    {
      await this.startConnection(username);
      console.log('SignalR connection established.');
    }
    if (this.hubConnection) {
      this.hubConnection
        .invoke('SendMessage', username, recipient, content)
        .catch((err) => console.error(err));
    }
  }

  onReceiveMessage(callback: (username: string, content: string,timestamp : Date) => void): void {
    this.hubConnection?.on('ReceiveMessage', callback);
    console.log('ReceiveMessage');
  }
  onUserListUpdate(callback: (users: string[]) => void): void {
    this.hubConnection?.on('UpdateUserList', callback);
    console.log('UpdateUserList');
  }
  getMessages(username :string, receiver: string) {
    // Call backend API to fetch message history for the selected user
    return this.translationService.getMessagesBySender(username,receiver);
  }
  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        console.log('SignalR connection stopped.');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

}
