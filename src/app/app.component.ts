import { Component, computed, signal } from '@angular/core';
import { environment } from '../environments/environment';

@Component({
  selector: 'giri-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  protected readonly environmentName = signal(environment.name);
  protected readonly message = computed(() => `Hello World - ${this.environmentName()}`);
  protected readonly insertText = computed(() => `Hello from ${this.environmentName()}`);
  protected readonly config = environment;
  protected readonly isInserting = signal(false);
  protected readonly statusMessage = signal<string | null>(null);

  protected async insertParagraph(): Promise<void> {
    this.isInserting.set(true);
    this.statusMessage.set(null);

    try {
      await Word.run(async (context) => {
        const selection = context.document.getSelection();
        selection.insertParagraph(this.insertText(), Word.InsertLocation.after);
        await context.sync();
      });

      this.statusMessage.set('Paragraph inserted.');
    } catch (error) {
      console.error('Unable to insert paragraph into Word document.', error);
      this.statusMessage.set('Unable to insert paragraph. Confirm this add-in is open in Microsoft Word.');
    } finally {
      this.isInserting.set(false);
    }
  }
}
