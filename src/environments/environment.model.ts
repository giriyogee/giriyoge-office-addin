export interface AddinEnvironment {
  readonly name: 'Production' | 'Local' | 'Dev' | 'UAT';
  readonly production: boolean;
  readonly sourceLocation: string;
}
