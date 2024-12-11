import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { ConfigService } from './config.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';


@Injectable({
  providedIn: 'root'
})

export class TranslationService {
  // private testurl = 'https://jsonplaceholder.typicode.com/posts'; 
  // private apiUrl = 'https://localhost:7257/api/translation/translate'; 
  private testUrl: string;
  private translateUrl: string;
  constructor(private http: HttpClient, @Inject(ConfigService) private configService: ConfigService) {
    this.testUrl = this.configService.get('apiEndpoints').testUrl;
    this.translateUrl = this.configService.get('apiEndpoints').translationApiUrl;
  }

  // Example method to fetch posts
  fetchPosts(): Observable<any> {
    return this.http.get(this.testUrl).pipe(
      catchError((error) => {
        console.error('Error fetching posts:', error);
        return throwError(() => new Error('Failed to fetch posts. Please try again.'));
      })
    );
  }

  // Modified translateText method to follow a similar pattern as fetchPosts
  translateText(text: string): Observable<any> {
    console.log('URL:', this.translateUrl);
    const url = `${this.translateUrl}/translate`;
    return this.http.post(url, { text }).pipe( // Send text object directly
      catchError((error) => {
        console.error('An error occurred:', error);
        return throwError(() => new Error('Failed to translate text. Please try again.'));
      })
    );
  }
  getMessagesBySender(sender: string,receiver: string): Observable<any> {
    const url = `${this.translateUrl}/GetMessagesByReceiver?user=${sender}&receiver=${receiver}`;
    console.log('URL:', url);
    return this.http.get(url).pipe(
      catchError((error) => {
        console.error('Error fetching messages by sender:', error);
        return throwError(() => new Error('Failed to fetch messages. Please try again.'));
      })
    );
  }
}