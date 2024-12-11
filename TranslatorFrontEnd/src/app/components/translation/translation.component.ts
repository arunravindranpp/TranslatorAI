import { Component,OnInit  } from '@angular/core';
import { TranslationService } from '../../services/translation.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-translation',
  templateUrl: './translation.component.html',
  styleUrls: ['./translation.component.css'],
  standalone: true,
  imports: [FormsModule] 
})
export class TranslationComponent implements OnInit {
  posts: any[] = [];
  inputText: string = '';
  translatedText: string = '';
  isLoading: boolean = false;

  constructor(private translationService: TranslationService) {}
  
  ngOnInit(): void {
    // this.translationService.fetchPosts().subscribe({
    //   next: (data) => {
    //     this.posts = data;
    //     console.log('Posts:', this.posts);
    //   },
    //   error: (error) => {
    //     console.error('Error fetching posts:', error);
    //   }
    // });
  }
  translate(): void {
    if (!this.inputText.trim()) {
      console.warn('Input text is empty');
      return;
    }

    this.isLoading = true;
    this.translationService.translateText(this.inputText).subscribe({
      next: (response: { translatedText: string }) => {
        this.translatedText = response.translatedText;
      },
      error: (error: any) => {
        console.error('Translation error:', error);
        alert('An error occurred while translating. Please try again.');
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
