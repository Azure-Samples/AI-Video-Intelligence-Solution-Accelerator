// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';
import { PanelMsg } from 'components/pages/dashboard/panel';
import './insightsImage.scss';

export class InsightsImage extends Component {

  handleImageLoaded() {
    const imgWidth =  this.refs.imageRef.width;
    const imgHeight = this.refs.imageRef.height;
    this.refs.canvasRef.width = imgWidth;
    this.refs.canvasRef.height = imgHeight;
    const canvasContext = this.refs.canvasRef.getContext("2d");

    this.props.boundingBoxes.forEach((bb) => {
      const bbymin = bb.data.bbymin;
      const bbxmin = bb.data.bbxmin;
      const bbymax = bb.data.bbymax;
      const bbxmax = bb.data.bbxmax;

      var x = imgWidth * bbxmin;
      var y = imgHeight * bbymin;
      var width = imgWidth * (bbxmax - bbxmin);
      var height = imgHeight * (bbymax - bbymin);

      // draw the bounding box
      canvasContext.strokeStyle = "#517B82";
      canvasContext.lineWidth = 2;
      canvasContext.strokeRect(x, y, width, height);

      // the label
      const labelHeight = 15;
      const padding = 4;
      const text = bb.data.cls + " | " + bb.data.score.toFixed(3);
      x = x-padding;
      y = y-labelHeight-(padding*2);
      // Draw the label
      const labelWidth = canvasContext.measureText(text).width + (padding*2);
      canvasContext.fillStyle = "#517B82"
      canvasContext.fillRect(x, y, labelWidth, labelHeight+(padding*2));
      // Draw the border
      canvasContext.lineWidth = 1;
      canvasContext.strokeStyle = "#41474b";
      canvasContext.strokeRect(x, y, labelWidth, labelHeight+(padding*2));
      // write the label
      canvasContext.textBaseline = "top";
      canvasContext.lineWidth = 0.25;
      canvasContext.strokeStyle = "#FFFFFF";
      canvasContext.strokeText(text, x+padding, y+(padding*2));
    });
  }

  render() {
    const { t, image } = this.props;
    if (!image) {
      return (
        <PanelMsg>{t('dashboard.noData')}</PanelMsg>
      );
    }

    return (
      <div ref="containerRef" className="insights-image-container">
        <img
          ref="imageRef"
          className="insights-image"
          src={image.url}
          alt={image.url}
          onLoad={this.handleImageLoaded.bind(this)}/>
        <canvas ref="canvasRef" width={100} height={100} />
      </div>
    );

  }
}
