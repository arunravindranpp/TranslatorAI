import { NgModule } from '@angular/core';
import { BrowserModule, provideClientHydration } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { provideHttpClient } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TranslationComponent } from './components/translation/translation.component';

@NgModule({
  declarations: [
    AppComponent,
    TranslationComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [
    provideClientHydration(),
    provideHttpClient(), // Provide HttpClient in the app module
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
