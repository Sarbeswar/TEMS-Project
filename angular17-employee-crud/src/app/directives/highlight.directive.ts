import { Directive, ElementRef, Input, OnChanges, Renderer2, SimpleChanges } from '@angular/core';

@Directive({
  selector: '[appHighlight]',
  standalone: true
})
export class HighlightDirective implements OnChanges {
  @Input() appHighlight = false;

  constructor(private readonly elementRef: ElementRef, private readonly renderer: Renderer2) {}

  ngOnChanges(_: SimpleChanges): void {
    this.renderer.setStyle(
      this.elementRef.nativeElement,
      'background',
      this.appHighlight ? '#ecfdf5' : 'transparent'
    );
  }
}
