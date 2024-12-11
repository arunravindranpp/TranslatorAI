import { Component, OnInit } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { ImpersonationComponent } from "../impersonation/impersonation.component";

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule, ImpersonationComponent]
})
export class ChatComponent implements OnInit {
  username = ''; // Current user's username
  message = ''; // Message input
  messages: { username: string; content: string ,timestamp:Date}[] = []; // Messages displayed in UI
  selectedUser: string | null = null; // Currently selected user
  //users: string[] = ['user1','user2','user3','user4']; // List of users
  users: string[]=[];
  isLoggedIn = false; // Flag to check if the user is logged in

  constructor(private chatService: ChatService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    // Check if username is passed in query params
    this.route.queryParams.subscribe(params => {
      if (params['username']) {
        this.username = params['username'];
        this.isLoggedIn = true;
        this.startChat();
      }
    });
  }
  shouldShowDateDivider(currentTimestamp: Date, index: number): boolean {
    if (index === 0) {
      return true; // Show date for the first message
    }
  
    const previousTimestamp = new Date(this.messages[index - 1].timestamp);
    const currentDate = new Date(currentTimestamp).toDateString();
    const previousDate = previousTimestamp.toDateString();
  
    return currentDate !== previousDate; // Show date if it's a new day
  }
  
  async startChat() {
    try {
      // Start the SignalR connection
      await this.chatService.startConnection(this.username);
      console.log('SignalR connection established.');
  
      // Now that the connection is established, fetch active users
      const activeUsers = await this.chatService.getActiveUsers();
      this.users = activeUsers.filter(user => user !== this.username); // Exclude self
      console.log('Initial active users:', this.users);
      if (this.users.length > 0) {
        this.selectUser(this.users[0]);
      }
      // Listen for updates to the user list
      this.chatService.onUserListUpdate((userList: string[]) => {
        this.users = userList.filter(user => user !== this.username); // Exclude self
        console.log('Updated user list:', this.users);
      });
  
      // Listen for incoming messages
      this.chatService.onReceiveMessage((username, content,timestamp) => {
        console.log('chatService.onReceiveMessage:', username, content,timestamp);
        if (this.selectedUser === username || username === this.username || this.selectedUser === null) {
          this.messages.push({ username, content,timestamp });
        }
        if (!this.users.includes(username) && username !== this.username) {
          this.users.push(username);
        }
      });
    } catch (error) {
      console.error('Error starting chat:', error);
    }
  }
  
  login() {
    if (this.username.trim()) {
      this.isLoggedIn = true;
      this.router.navigate(['/chat'], { queryParams: { username: this.username } });
      this.startChat();
    }
  }
  logout() {
    this.isLoggedIn = false;
    this.username = ''; // Clear the username
    this.selectedUser = null; // Deselect any user
    this.messages = []; // Clear chat messages
    this.users = []; // Clear the user list
    this.chatService.stopConnection(); // Optionally stop SignalR connection
    this.router.navigate(['/']); // Navigate to the root page, or a desired page
  }

  sendMessage() {
    if (this.username && this.message) {
      const recipient = this.selectedUser || ''; // Determine recipient (empty for public)
      this.chatService.sendMessage(this.username, recipient, this.message);

      this.messages.push({
        username: this.username,
        content: this.message,
        timestamp: new Date()
      });

      this.message = '';
    }
  }

  selectUser(user: string) {
    this.selectedUser = user;
    this.messages = []; // Clear chat window when switching users
    this.loadMessageHistory(this.username,user); // Load message history
  }
  
  async loadMessageHistory(username:string,receiver: string) {
    try {
      const messagesData = await firstValueFrom(this.chatService.getMessages(username,receiver));
      const historyMessages = messagesData.map((msg: { sender: string; messageText: string; timestamp: string }) => ({
        username: msg.sender,
        content: msg.messageText,
        timestamp: new Date(msg.timestamp),
      }));
  
      this.messages = [...historyMessages];
      console.log('Message history loaded:', this.messages);
    } catch (error) {
      console.error('Error loading message history:', error);
    }
  }
}
