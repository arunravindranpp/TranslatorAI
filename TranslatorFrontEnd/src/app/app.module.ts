import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserModule, provideClientHydration } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { provideHttpClient } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TranslationComponent } from './components/translation/translation.component';
import { ConfigService } from './services/config.service';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ChatComponent } from './components/chat/chat.component';
import { ImpersonationComponent } from './components/impersonation/impersonation.component';

export function initializeConfig(configService: ConfigService) {
  return () => configService.loadConfig();
}

@NgModule({
  declarations: [
    AppComponent // Keep only the root component here
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    TranslationComponent,
    ChatComponent,
    ImpersonationComponent // Import the standalone component here
  ],
  providers: [
    provideClientHydration(),
    provideHttpClient(),
    ConfigService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeConfig,
      deps: [ConfigService],
      multi: true,
    },
    provideAnimationsAsync()
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
