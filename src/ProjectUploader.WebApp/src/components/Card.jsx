import React from 'react';

const Card = ({ children, className = '', title }) => {
  return (
    <div className={`glass-panel p-6 ${className}`} style={{ padding: '1.5rem' }}>
      {title && <h3 className="text-xl font-semibold mb-4 border-b border-[var(--border-color)] pb-2">{title}</h3>}
      <div>{children}</div>
    </div>
  );
};

export default Card;
