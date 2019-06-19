// Copyright (c) Microsoft. All rights reserved.

import React from 'react';

import { joinClasses } from 'utilities';

import './panel.scss';

export const Panel = ({ className, children }) => (
  <div className={joinClasses('panel-container', className)}>
    { children }
  </div>
);
